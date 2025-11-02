
## Table of Contents

1. [Authentication](#authentication)
2. [Account APIs](#account-apis)
3. [Order APIs](#order-apis)
4. [Response Format](#response-format)
5. [Error Handling](#error-handling)
6. [Enums](#enums)

---

## Authentication

The API uses **JWT (JSON Web Tokens)** for authentication. Most endpoints require authentication, except where explicitly noted.

### Token Structure

- **Access Token**: Valid for 30 minutes
- **Refresh Token**: Valid for 7 days
- **Token Format**: Bearer token in Authorization header

### Authentication Header

Include the access token in the request header:

```
Authorization: Bearer <access_token>
```

---

## Account APIs

### 1. Register Admin User

Register a new admin user (test endpoint).

**Endpoint:** `POST /api/account/register`  
**Authentication:** Not required

**Request Body:**
```json
{
  "email": "admin@example.com",
  "password": "YourPassword123!"
}
```

**Request Validation:**
- `email`: Required, must be valid email format
- `password`: Required

**Password Requirements:**
- Minimum 6 characters
- At least one digit
- At least one uppercase letter
- At least one lowercase letter
- At least one non-alphanumeric character

**Success Response (201):**
```json
{
  "isSuccess": true,
  "message": "user registerd !!",
  "data": null,
  "error": null
}
```

**Error Response (400):**
```json
{
  "isSuccess": false,
  "message": "this email already registerd",
  "data": null,
  "error": null
}
```

---

### 2. Login

Authenticate and receive access and refresh tokens.

**Endpoint:** `POST /api/account/login`  
**Authentication:** Not required

**Request Body:**
```json
{
  "email": "admin@example.com",
  "password": "YourPassword123!"
}
```

**Success Response (200):**
```json
{
  "isSuccess": true,
  "message": "successful Authentication",
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "base64_encoded_refresh_token",
    "expiresAt": "2024-01-01T12:30:00Z"
  },
  "error": null
}
```

**Error Response (401):**
```json
{
  "isSuccess": false,
  "message": "Invalid Email or Password",
  "data": null,
  "error": null
}
```

---

### 3. Refresh Token

Obtain a new access token using a valid refresh token.

**Endpoint:** `POST /api/account/refresh-token`  
**Authentication:** Not required

**Request Body:**
```json
{
  "refreshToken": "base64_encoded_refresh_token"
}
```

**Success Response (200):**
```json
{
  "isSuccess": true,
  "message": "Token refreshed successfully",
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "new_base64_encoded_refresh_token",
    "expiresAt": "2024-01-01T12:30:00Z"
  },
  "error": null
}
```

**Error Response (401):**
```json
{
  "isSuccess": false,
  "message": "Invalid or expired refresh token",
  "data": null,
  "error": null
}
```

**Note:** The old refresh token will be revoked after successful refresh.

---

### 4. Forgot Password

Request a password reset OTP via email.

**Endpoint:** `POST /api/account/forgot-password`  
**Authentication:** Not required  
**Rate Limit:** 5 requests per 5 minutes per IP address

**Request Body:**
```json
{
  "email": "admin@example.com"
}
```

**Success Response (200):**
```json
{
  "isSuccess": true,
  "message": "If your email exists, an OTP will be sent.",
  "data": null,
  "error": null
}
```

**Note:** The API returns the same message regardless of whether the email exists (security best practice). OTP is valid for 10 minutes.

**Rate Limit Response (429):**
If you exceed the rate limit, you'll receive a 429 Too Many Requests status.

---

### 5. Verify OTP and Reset Password

Verify the OTP received via email and reset the password.

**Endpoint:** `POST /api/account/verify-otp-reset-password`  
**Authentication:** Not required  
**Rate Limit:** 5 requests per 5 minutes per IP address

**Request Body:**
```json
{
  "email": "admin@example.com",
  "otp": "123456",
  "newPassword": "NewPassword123!"
}
```

**Request Validation:**
- `email`: Required
- `otp`: Required (6-digit code)
- `newPassword`: Required, must meet password requirements

**Success Response (200):**
```json
{
  "isSuccess": true,
  "message": "Password reset successfully.",
  "data": null,
  "error": null
}
```

**Error Response (400):**
```json
{
  "isSuccess": false,
  "message": "Invalid or expired OTP.",
  "data": null,
  "error": null
}
```

**Note:** The OTP can only be used once and expires after 10 minutes.

---

## Order APIs

### 1. Create Order

Create a new translation order. This endpoint is public and does not require authentication.

**Endpoint:** `POST /makeOrder`  
**Authentication:** Not required

**Content-Type:** `multipart/form-data`

**Request Body (Form Data):**
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `customerName` | string | Yes | Customer's full name |
| `customerEmail` | string | Yes | Customer's email address |
| `customerPhoneNumber` | string | Yes | Customer's phone number |
| `deadLine` | datetime | Yes | Order deadline (ISO 8601 format) |
| `notes` | string | No | Additional notes about the order |
| `pageCount` | integer | Yes | Number of pages to translate |
| `wordCount` | integer | No | Number of words to translate |
| `preferredContact` | string | Yes | Preferred contact method (see [Enums](#enums)) |
| `services` | array | Yes | List of services required |
| `sourceLanguage` | string | Yes | Source language code |
| `targetLanguage` | string | Yes | Target language code |
| `file` | file | No | Document file to upload |

**Example Request (JavaScript/Fetch):**
```javascript
const formData = new FormData();
formData.append('customerName', 'John Doe');
formData.append('customerEmail', 'john@example.com');
formData.append('customerPhoneNumber', '+1234567890');
formData.append('deadLine', '2024-12-31T23:59:59Z');
formData.append('notes', 'Please handle with care');
formData.append('pageCount', '10');
formData.append('wordCount', '5000');
formData.append('preferredContact', 'Email');
formData.append('services', JSON.stringify(['Translation', 'Proofreading']));
formData.append('sourceLanguage', 'en');
formData.append('targetLanguage', 'ar');
formData.append('file', fileInput.files[0]); // File object

fetch('/makeOrder', {
  method: 'POST',
  body: formData
});
```

**Example Request (cURL):**
```bash
curl -X POST https://api.example.com/makeOrder \
  -F "customerName=John Doe" \
  -F "customerEmail=john@example.com" \
  -F "customerPhoneNumber=+1234567890" \
  -F "deadLine=2024-12-31T23:59:59Z" \
  -F "notes=Please handle with care" \
  -F "pageCount=10" \
  -F "wordCount=5000" \
  -F "preferredContact=Email" \
  -F "services=Translation" \
  -F "services=Proofreading" \
  -F "sourceLanguage=en" \
  -F "targetLanguage=ar" \
  -F "file=@document.pdf"
```

**Success Response (200):**
```json
{
  "isSuccess": true,
  "message": "the Order saved correctly",
  "data": {
    "id": 1,
    "customerName": "John Doe",
    "customerEmail": "john@example.com",
    "customerPhoneNumber": "+1234567890",
    "deadLine": "2024-12-31T23:59:59Z",
    "notes": "Please handle with care",
    "pageCount": 10,
    "wordCount": 5000,
    "orderStatus": "Pending",
    "preferredContact": "Email",
    "services": ["Translation", "Proofreading"],
    "sourceLanguage": "en",
    "targetLanguage": "ar",
    "uploadedFilePath": "/uploads/unique_filename.pdf"
  },
  "error": null
}
```

**Error Response (400):**
```json
{
  "isSuccess": false,
  "message": "Invalid Data",
  "data": null,
  "error": {
    // ModelState errors
  }
}
```

---

### 2. Get All Orders

Retrieve all orders (Admin only).

**Endpoint:** `POST /orders`  
**Authentication:** Required (Admin role)

**Request Headers:**
```
Authorization: Bearer <access_token>
```

**Request Body:** None

**Success Response (200):**
```json
{
  "isSuccess": true,
  "message": "Orderes retrived succesfully",
  "data": [
    {
      "id": 1,
      "customerName": "John Doe",
      "customerEmail": "john@example.com",
      "customerPhoneNumber": "+1234567890",
      "deadLine": "2024-12-31T23:59:59Z",
      "notes": "Please handle with care",
      "pageCount": 10,
      "wordCount": 5000,
      "orderStatus": "Pending",
      "preferredContact": "Email",
      "services": ["Translation", "Proofreading"],
      "sourceLanguage": "en",
      "targetLanguage": "ar",
      "uploadedFilePath": "/uploads/file.pdf"
    }
    // ... more orders
  ],
  "error": null
}
```

**Error Response (401):**
```json
{
  "isSuccess": false,
  "message": "Unauthorized",
  "data": null,
  "error": null
}
```

---

### 3. Update Order Status

Update the status of an existing order (Admin only).

**Endpoint:** `PATCH /orders/{orderId}`  
**Authentication:** Required (Admin role)

**URL Parameters:**
- `orderId` (integer, required): The ID of the order to update

**Request Headers:**
```
Authorization: Bearer <access_token>
Content-Type: application/json
```

**Request Body:**
```json
{
  "orderStatus": "WorkingON"
}
```

**Valid Order Status Values:**
- `Pending`
- `WorkingON`
- `Done`

**Success Response (200):**
```json
{
  "isSuccess": true,
  "message": "Order status updated successfully",
  "data": {
    "id": 1,
    "customerName": "John Doe",
    "customerEmail": "john@example.com",
    "orderStatus": "WorkingON",
    // ... other order fields
  },
  "error": null
}
```

**Error Response (404):**
```json
{
  "isSuccess": false,
  "message": "Order not found",
  "data": null,
  "error": null
}
```

**Error Response (400):**
```json
{
  "isSuccess": false,
  "message": "Invalid Data",
  "data": null,
  "error": {
    // ModelState errors
  }
}
```

**Error Response (401):**
```json
{
  "isSuccess": false,
  "message": "Unauthorized",
  "data": null,
  "error": null
}
```

---

## Response Format

All API responses follow a consistent format:

```json
{
  "isSuccess": boolean,
  "message": string,
  "data": object | array | null,
  "error": object | null
}
```

### Response Fields

- **isSuccess**: Indicates whether the request was successful
- **message**: Human-readable message describing the result
- **data**: The response payload (varies by endpoint)
- **error**: Error details (only present when `isSuccess` is false)

---

## Error Handling

### HTTP Status Codes

| Status Code | Meaning | Description |
|-------------|---------|-------------|
| 200 | OK | Request successful |
| 201 | Created | Resource created successfully |
| 400 | Bad Request | Invalid request data or validation errors |
| 401 | Unauthorized | Authentication required or invalid credentials |
| 404 | Not Found | Resource not found |
| 429 | Too Many Requests | Rate limit exceeded |
| 500 | Internal Server Error | Server error |

### Error Response Example

```json
{
  "isSuccess": false,
  "message": "Invalid Data",
  "data": null,
  "error": {
    "Email": ["The Email field is required."],
    "Password": ["The Password field must be at least 6 characters."]
  }
}
```

---

## Enums

### OrderStatus

Enum values for order status:

- `Pending` - Order is pending
- `WorkingON` - Order is being processed
- `Done` - Order is completed

### PreferredContact

Enum values for preferred contact method:

- `PhoneCall` - Contact via phone call
- `Email` - Contact via email
- `Whatsapp` - Contact via WhatsApp

**Note:** When sending enum values in requests, you can use either the enum name (e.g., "Email") or the numeric value. The API accepts case-insensitive enum names.

---

## CORS Configuration

The API is configured to accept requests from any origin. CORS headers are automatically included in responses.

---

## Rate Limiting

The following endpoints have rate limiting:

- **Forgot Password**: 5 requests per 5 minutes per IP address
- **Verify OTP Reset Password**: 5 requests per 5 minutes per IP address

When rate limit is exceeded, the API returns HTTP status `429 Too Many Requests`.

---

## File Uploads

### Supported File Types

The order creation endpoint accepts file uploads. Files are stored in the `/uploads` directory with unique names to prevent conflicts.

### File Path in Response

When a file is uploaded, the response includes `uploadedFilePath` with a relative path like `/uploads/unique_filename.pdf`. You can access the file by appending this path to your base URL.

### File Size

Currently, there's no explicit file size limit documented. Contact the backend team for specific limits.

---

## Authentication Flow

### Recommended Flow

1. **Login**: Call `/api/account/login` with credentials
2. **Store Tokens**: Save both `accessToken` and `refreshToken` securely
3. **Use Access Token**: Include `accessToken` in Authorization header for protected endpoints
4. **Refresh When Needed**: When access token expires (or before expiration), call `/api/account/refresh-token` with the `refreshToken`
5. **Handle Errors**: If refresh fails, redirect user to login page

### Example Implementation (JavaScript)

```javascript
// Login
const loginResponse = await fetch('/api/account/login', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({ email, password })
});

const { data } = await loginResponse.json();
localStorage.setItem('accessToken', data.accessToken);
localStorage.setItem('refreshToken', data.refreshToken);

// Make authenticated request
const response = await fetch('/orders', {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${localStorage.getItem('accessToken')}`
  }
});

// Refresh token when needed
const refreshResponse = await fetch('/api/account/refresh-token', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    refreshToken: localStorage.getItem('refreshToken')
  })
});
```

---

## Testing

The API includes Swagger documentation when running in Development mode. Access it at:

```
/swagger
```

Use Swagger UI to test endpoints interactively.

---

## Support

For questions or issues, please contact the backend development team.

---

**Last Updated:** 2024


# How to Fix "SmtpClientAuthentication is disabled" Error

## Problem
Office 365 has SMTP AUTH (legacy authentication) disabled at the tenant level. This is a security setting that blocks basic username/password SMTP authentication.

## Solution 1: Enable SMTP AUTH (Recommended - If you have Admin Access)

### Option A: Enable for Specific Mailbox (Easiest)

1. **Connect to Exchange Online PowerShell:**
   ```powershell
   Install-Module -Name ExchangeOnlineManagement
   Connect-ExchangeOnline
   ```

2. **Enable SMTP AUTH for your mailbox:**
   ```powershell
   Set-CASMailbox -Identity "a.samy@ash-translation.co.uk" -SmtpClientAuthenticationDisabled $false
   ```

3. **Verify it's enabled:**
   ```powershell
   Get-CASMailbox -Identity "a.samy@ash-translation.co.uk" | Select SmtpClientAuthenticationDisabled
   ```
   Should return `False` if enabled.

### Option B: Enable for Entire Tenant (Requires Global Admin)

1. **Connect to Exchange Online PowerShell** (as above)

2. **Enable SMTP AUTH for the tenant:**
   ```powershell
   Set-TransportConfig -SmtpClientAuthenticationDisabled $false
   ```

3. **Verify:**
   ```powershell
   Get-TransportConfig | Select SmtpClientAuthenticationDisabled
   ```

### Option C: Enable via Office 365 Admin Center (UI)

1. Go to https://admin.microsoft.com
2. Navigate to **Settings** → **Org settings** → **Mail**
3. Find **POP and IMAP** settings
4. Enable **Authenticated SMTP** for users who need it

**Note:** This may take a few minutes to propagate after enabling.

---

## Solution 2: Use OAuth2 Authentication (More Secure)

If you cannot enable SMTP AUTH or want a more secure solution, you can use OAuth2 authentication. This requires Azure app registration and is more complex but doesn't require SMTP AUTH to be enabled.

**This requires:**
- Azure App Registration
- Client ID and Client Secret
- OAuth2 token acquisition
- Code changes to use OAuth2 instead of basic auth

---

## Solution 3: Use Alternative Email Service

If you cannot enable SMTP AUTH and don't want to implement OAuth2, consider using a different email service that supports SMTP AUTH by default:

- **SendGrid** (Free tier available)
- **Mailgun** (Free tier available)
- **Amazon SES** (Pay-as-you-go)
- **Gmail** (If using a personal account with App Password)

---

## Quick Test After Enabling SMTP AUTH

After enabling SMTP AUTH, wait 5-10 minutes for the changes to propagate, then try sending an email again. The error should be resolved.

## Need Help?

If you don't have admin access to enable SMTP AUTH, contact your Office 365 administrator with this error message:
```
535: 5.7.139 Authentication unsuccessful, SmtpClientAuthentication is disabled for the Tenant
```

They can enable SMTP AUTH for your mailbox or the entire tenant.


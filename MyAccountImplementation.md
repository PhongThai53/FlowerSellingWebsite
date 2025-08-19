# My Account Feature Implementation

## Overview

I have successfully implemented a comprehensive my-account feature for your flower selling website with both backend and frontend components.

## Backend Implementation

### 1. Created Password Validation Attribute

- **File**: `Backend/Attributes/PasswordValidationAttribute.cs`
- **Purpose**: Custom validation for strong passwords
- **Requirements**: 8+ characters, 1 lowercase, 1 uppercase, 1 number, 1 special character

### 2. Enhanced DTOs

- **UpdateAccountRequestDTO**: For account profile updates
- **Enhanced ChangePasswordRequestDTO**: With strong password validation
- **Updated existing DTOs**: Added proper validation attributes

### 3. Created MyAccountController

- **File**: `Backend/Controllers/MyAccountController.cs`
- **Endpoints**:
  - `GET /api/MyAccount/profile` - Get current user profile
  - `PUT /api/MyAccount/profile` - Update current user profile
  - `POST /api/MyAccount/change-password` - Change password
  - `GET /api/MyAccount/address` - Get current user address

### 4. Enhanced UserService

- **Added validation**: Prevents setting new password same as current password
- **Error handling**: Proper exception handling with descriptive messages

## Frontend Implementation

### 1. My Account Page

- **File**: `frontend/wwwroot/html/user/my-account.html`
- **Features**:
  - Dashboard with user welcome message
  - Address display section
  - Account details form (Full Name, Username, Email, Phone, Address)
  - Password change form with validation
  - Logout functionality

### 2. JavaScript Manager

- **File**: `frontend/wwwroot/js/my-account.js`
- **Features**:
  - Authentication checking
  - Profile loading and updating
  - Password change with client-side validation
  - Address management
  - Automatic logout after password change
  - Error handling and user feedback

### 3. Enhanced Styling

- **File**: `frontend/wwwroot/css/user.css`
- **Features**:
  - Modern, responsive design
  - Gradient backgrounds and smooth transitions
  - Form validation states
  - Loading animations
  - Mobile-responsive layout

### 4. Updated Navigation

- **File**: `frontend/wwwroot/html/components/header.html`
- **Added**: My Account link in the navigation menu

## Features Implemented

### Dashboard Tab

- Displays personalized welcome message
- Shows user's full name
- Overview of account functionality

### Address Tab

- Shows current user address information
- Displays full name, address, and phone number
- "Edit Address" button that switches to Account Details tab
- Handles empty address states gracefully

### Account Details Tab

- **Editable Fields**:
  - Full Name (required, min 2 characters)
  - Username (required, min 3 characters, alphanumeric + underscores)
  - Email (required, valid email format)
  - Phone (optional, phone number validation)
  - Address (optional, max 200 characters)
- **Validation**: Client-side and server-side validation
- **Auto-save**: Updates address display after saving

### Change Password Tab

- **Current Password**: Required field
- **New Password**: Strong password validation
- **Confirm Password**: Must match new password
- **Security**: Prevents using same password as current
- **Auto-logout**: Automatically logs out after successful password change

### Security Features

- **Authentication Check**: Redirects to login if not authenticated
- **Token Validation**: Validates JWT tokens on all requests
- **Password Strength**: Enforced strong password policy
- **Same Password Prevention**: Backend validation prevents reusing current password

## Testing Guide

### Prerequisites

1. Ensure backend is running on `https://localhost:7066`
2. Ensure authentication system is working
3. Have a valid user account with JWT token

### Test Cases

#### 1. Authentication Test

- Navigate to `/html/user/my-account.html` without authentication
- Should redirect to login page

#### 2. Profile Loading Test

- Login with valid credentials
- Navigate to my-account page
- Verify dashboard shows correct username
- Verify account details form is populated with user data

#### 3. Account Update Test

- Modify full name, phone, or address
- Click "Save Changes"
- Verify success message appears
- Verify address tab reflects changes

#### 4. Password Change Test

- Enter current password
- Enter new password meeting requirements
- Enter matching confirm password
- Click "Change Password"
- Verify success message and automatic logout

#### 5. Validation Tests

- Try invalid email format
- Try username less than 3 characters
- Try weak password
- Try mismatched password confirmation
- Verify appropriate error messages

#### 6. Address Display Test

- Check address tab shows current information
- Test with empty address data
- Test "Edit Address" button functionality

### API Endpoints Testing

#### Get Profile

```bash
GET https://localhost:7066/api/MyAccount/profile
Authorization: Bearer {your-jwt-token}
```

#### Update Profile

```bash
PUT https://localhost:7066/api/MyAccount/profile
Authorization: Bearer {your-jwt-token}
Content-Type: application/json

{
  "fullName": "Updated Name",
  "userName": "updateduser",
  "email": "updated@email.com",
  "phone": "+1234567890",
  "address": "123 Updated St, City, State"
}
```

#### Change Password

```bash
POST https://localhost:7066/api/MyAccount/change-password
Authorization: Bearer {your-jwt-token}
Content-Type: application/json

{
  "currentPassword": "OldPassword123!",
  "newPassword": "NewPassword456@",
  "confirmPassword": "NewPassword456@"
}
```

#### Get Address

```bash
GET https://localhost:7066/api/MyAccount/address
Authorization: Bearer {your-jwt-token}
```

## Key Features Compliance

✅ **Dashboard**: Shows current user address and account info  
✅ **Account Details**: Full user information editing with validation  
✅ **Change Password**: Strong password validation, logout after change  
✅ **Address Display**: Current user address visibility  
✅ **Backend Validation**: All fields validated with proper error messages  
✅ **Password Policy**: Cannot reuse current password, strength requirements  
✅ **Error Handling**: Comprehensive error messages pushed to frontend

## Files Created/Modified

### Backend Files

- `Backend/Attributes/PasswordValidationAttribute.cs` (NEW)
- `Backend/Controllers/MyAccountController.cs` (NEW)
- `Backend/Models/DTOs/UpdateAccountRequestDTO.cs` (NEW)
- `Backend/Models/DTOs/ChangePasswordRequestDTO.cs` (MODIFIED)
- `Backend/Services/Implementations/UserService.cs` (MODIFIED)

### Frontend Files

- `frontend/wwwroot/html/user/my-account.html` (NEW)
- `frontend/wwwroot/js/my-account.js` (NEW)
- `frontend/wwwroot/css/user.css` (MODIFIED)
- `frontend/wwwroot/html/components/header.html` (MODIFIED)

The implementation is complete and ready for testing!

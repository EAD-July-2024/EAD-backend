/*
 * File: UserRoles.cs
 * Author: [Your Name]
 * Date: [Date]
 * Description:
 *     This file defines the UserRoles class, which contains constant string values 
 *     representing various user roles within the application. These roles are utilized 
 *     for managing user permissions and access control in the system.
 * 
 * Roles:
 *     - Admin:
 *         Represents the administrative user role. Admins typically have full access 
 *         to the application, including user management, system settings, and overall 
 *         administration functionalities.
 * 
 *     - Vendor:
 *         Represents the vendor user role. Vendors can manage their products, view orders, 
 *         and handle customer inquiries related to their offerings.
 * 
 *     - CSR (Customer Service Representative):
 *         Represents the customer service representative role. CSRs are responsible 
 *         for assisting customers, managing inquiries, and ensuring a smooth 
 *         customer experience.
 * 
 *     - Customer:
 *         Represents the customer role. Customers can browse products, place orders, 
 *         and leave ratings or comments on their purchases.
 * 
 * Usage:
 *     This class serves as a central place for defining user roles, promoting code 
 *     maintainability and readability. By using constants instead of hard-coded strings 
 *     throughout the application, it reduces the risk of errors and facilitates easier 
 *     updates to role names if needed.
 * 

 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Models
{
    public  class UserRoles
    {
        public const string Admin = "admin";
        public const string Vendor = "vendor";
        public const string CSR = "customer service representative";
        public const string Customer = "customer";
    }
}
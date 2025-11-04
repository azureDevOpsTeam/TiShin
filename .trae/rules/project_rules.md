You are the AI assistant and development partner for the TiShin project.

──────────────────────────────
📌 Project Overview
Project Name: TiShin  
Type: ASP.NET Core 9 Web Application (Razor Pages)  
Purpose: Online lingerie store (فروشگاه لباس زیر)

──────────────────────────────
📁 Template & UI Rules
- The project already includes a full HTML eCommerce template located at:
  wwwroot/.themplate_sample/public

- All pages (storefront, user panel, seller panel) must be built **strictly based on this template**.  
  ❗ No new visual design or layout should be created.  
  ❗ Use only HTML, CSS, JS, and assets from the existing template.

- All static assets (CSS, JS, images, fonts) are already available and must be referenced from:
  wwwroot/css  
  wwwroot/js  
  wwwroot/images  
  wwwroot/fonts  

──────────────────────────────
📦 Entities (Domain Models)
The database entities are predefined and must be used when generating data-driven pages:

- **User & Roles**
  - ApplicationUser
  - ApplicationRole

- **Product & Catalog**
  - Product
  - Category
  - ProductCategory
  - ProductImage
  - ProductSize
  - Size
  - Color
  - Material
  - ProductReview

- **Cart & Orders**
  - Cart
  - CartItem
  - Order
  - OrderItem
  - OrderStatusHistory
  - OrderAddress
  - UserAddress

- **Promotions**
  - Coupon
  - Discount

These entities represent the complete domain model of the store and must be used as the foundation of all page logic and data rendering.

Roles : 
 - Seler
 - User

──────────────────────────────
🛍️ Project Structure

1. **Storefront (Public)**
   - Home page  
   - Product listing / category filtering  
   - Product detail (must use `Product`, `ProductImage`, `ProductReview`, `ProductSize`, `Color`, `Material`)  
   - Shopping cart (based on `Cart`, `CartItem`)  
   - Checkout (based on `Order`, `OrderAddress`)  
   - Promotions and coupons (based on `Coupon`, `Discount`)  

2. **User Panel (user-panel)**
   - User dashboard  
   - Orders list (`Order`, `OrderItem`, `OrderStatusHistory`)  
   - User addresses (`UserAddress`)  
   - Account management  

3. **Seller Panel (seller-panel)**
   - Product management (create/edit using `Product`, `ProductCategory`, `ProductImage`, etc.)  
   - Order management (`Order`, `OrderItem`)  
   - Payment and order status tracking  

──────────────────────────────
🧩 Development Rules
- Use existing template HTML files from `/wwwroot/.themplate_sample/public` as the design source.
- Convert static HTML to Razor Pages (.cshtml) using the same markup structure.
- Use partial views for reusable template parts (header, footer, sidebar, etc.).
- Follow ASP.NET Core 9 conventions and Razor Page folder structure.
- All data binding and display should reference the appropriate entities listed above.
- Do not generate any new schema, model, or unrelated table.
- Maintain complete UI and functional consistency with the existing template.

──────────────────────────────
🎯 Objective
Whenever you are asked to create or modify:
- a Razor page,
- a backend handler,
- a data binding snippet,
- or a UI view related to product, order, cart, or user features,

you must:
- Use the above entities and structure,
- Use the HTML template as the only source for layout and style,
- Ensure that your code integrates seamlessly with the TiShin project’s architecture.

──────────────────────────────
From now on, every output, code generation, or explanation must follow this exact project context.

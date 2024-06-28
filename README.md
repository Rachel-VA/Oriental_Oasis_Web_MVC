
# ASP.NET Core Web Application Setup



This guide will walk you through the steps to set up an ASP.NET Core C# web application and resolve the SSL certificate warning issue when running the application locally.

## Setup Instructions

- Visual Studio with the ASP.NET and web development workload

### 1. Install ASP.NET and Web Development Workload

 **Select the ASP.NET and Web Development Workload:**
   - In the workloads tab, check the "ASP.NET and web development" workload.
   - Click "Modify" or "Install" to apply the changes.



### 1. Create a New ASP.NET Core Web Application

1. **Open Visual Studio:**
   - Open Visual Studio after installing the necessary workload.

2. **Create a New Project:**
   - Click on "Create a new project" from the start window.

3. **Select the ASP.NET Core Web Application Template:**
   - Search for "ASP.NET Core Web App (Model-View-Controller)".
   - Select the template and click "Next".

4. **Configure Your Project:**
   - Name your project, choose a location, and click "Create".

5. **Configure Additional Settings:**
   - Select ".NET Core" and the latest ASP.NET Core version.
   - Ensure "MVC" is selected.
  
   - Click "Create".

### 3. Trust the HTTPS Development Certificate
To avoid SSL certificate warnings when running your application locally, you need to trust the HTTPS development certificate.

1. In the terminal, run the following command:
   .net dev-certs https --trust then restart the computer

### 4. Add Source Control from Visual Studio

- Select Add source control in the right corner of VS
- VS will initialize Git repo. Then give a name for the repo and click create and push

## Connect to Microsoft SQL Server Management Studio
- Download Microsoft SQL Server Management Studio
- Connect to a server
- Config in appsettings.json: "ConnectionStrings": {
    "DefaultConnection": "Server=RPSAVAGEMAIN;Database=RSavage;Trusted_Connection=True;TrustServerCertificate=True"
  }
- - go to Tools -NuGet Manager Console and type in 'update-database' to connect. The database name should be shown in the MS SQL database

### Install Dependencies
- Open NuGet Package Manager and install: 
- Microsoft.EntityFrameworkCore (version 8.0.6)
- Microsoft.EntityFrameworkCore.SqlServe (version 8.0.6)
- Microsoft.EntityFrameworkCore.Tools (version 8.0.6)


### Setup  Application Db Context for Entiry FrameWork Core
- Create a folder name it Data & - and a new cs file and name it (ApplicationDbContext.cs) inside the folder
- Enherent 'DbContext' from the Framework 
- Register the service 'AddDBContex' in Program.cs file

### Create a database
- Go to Tools - NuGet Manager Console and enter the command 'update-database' to create a table in the studio database
- Go to Database Studio and refresh it, the new name should appear in the database

### Create A Category Table
- Create a Db property for Category in ApplicationDbContext.cs file.
- Go to Tools -> NuGet Package Manager -> Package Manager Console to migrate it by entering the command add-migration AddCategoryTableToDb and press enter. It will auto-generate a migration folder and files in the project.
- Go to NuGet Package Manager Console and run update-database.
- To check for update migration in the database, right-click dbo._EFMigrationHistory and select top 1000 to see.

### Add Category Controller
- Right-click on the Controllers folder -> Add -> Controller -> select an empty controller and name it CategoryController.
- Right-click on the Views folder -> Add -> Folder -> name it Category.
- Right-click on the Category folder -> Add -> View -> select an empty razor view. It will generate an Index.cshtml file.


### Add Category Link in Header

- Go to Shared folder and open _Layout.cshtml and add in another nav tab and name it category

### Seed Category Table
- Go to the Data folder -> ApplicationDbContext.cs -> open it and create code to override the default function that the model generated.
- Open Console NuGet and run add-migration SeedCategoryTable to push the created data into the database.
- Then run the command update-database to update the database to see changes.
- To see the changes, go to the database, right-click on dbo.Category table.



### Repository Pattern for database
- Create class libraries as new projects: OrientalOasis.DataAccess, OrientalOasis.Model, and OrientalOasis.Utilities.
- Move Data and Migration folders into .DataAccess project, move Models folder into .Model, create a new class inside .Utilities project to store static data.

### Reset database
- Delete the database in SQL studio management
- In the project, delete the Migration folder
- Open Tools - NuGets manager console, select OreientalOasis.DataAccess, then using command: Add - migration AddCategoryToDbAndSeedTable
- Then update the database to push to updated data using : update - database

- ### Seperate the web in Areas
- on the project, right click to create a new Scaffolded Item and create an Area MVC Admin
- The Framework will auto generate a folder Admin with code files in it
- Copy the {area:exists} and past it in program.cs and name it customer to make it invoke default for customer interface
- Move CategoryController.cs file into Admin controller
- Move HomeController.cs to Customer controller
- Define asp-area views in _Layout.cshtml file for it to know where to find the controller_

### product CRUD operations
#### Create product model
- Create product item list and pupulate it
- Open NuGet Manager console and : add-migration addProductsToDb (make sure to select the .DataAcess)
- Run: update-database

#### Part 2: List of features in Product CRUD operations
- Create Product model
- Seed products 
- Create foreign keys
- Handle IMGs both in database & on view
- Create View model
- File I/O
- UpSert: Combine, create, edit pages
- Rich Text
- Create product

- Loading Nav
#### Datatable API
- Using DataTable Plugin API
- Go to datatable.net
- copy .css file add it to _Layout.cshtml
- copy .js file to _Layout.cshtml in the bottom
- Create API call in the end of ProductController.cs file
- nav to js folder and create a new item product.js
- create a document method to load the table
- copy the Ajax datatable and pasted it in product.js
- from datatable.net/data, copy the column code
- Database column
- Edit product link in dataTable
- Delete product
- sweet alert

#### Home Page
- Display products on Home Page
- Details action func
- Detail Page


### Part 3: Identity Management Using Identity.EntityFramework
- Ensure to install Microsoft.AspNetCore.Identity.EntityFrameworkCore package for both the main project and the sub-project .DataAccess.
- Ensure the app.UseAuthentication(); is called before the app.UseAuthorization(); in Program.cs.
- The framework will auto-generate a new ApplicationDbContext, remove it and pass in the IdentityUser in ApplicationDbContext.
- Check all the changes in the project after the Scaffolder Identity successfully configured.
- In appsettings.json, remove the newly generated ApplicationDbContext server.
- Check the Areas folder and delete the generated Data folder. Keep the Pages folder. It contains all the new files (razor views) for Identity (register, login, validations).
- Register the Razor view pages and add in the routing pipeline in Program.cs for the pages to be seen.

#### Create identity Table
- Open tools-NuGet management console and add migration "add-migration addIdentityTables" to create table in database to store user info
- Run update-database

#### Extend user identity (add more columns in database to store more user info)
- Create a new class inside .Model sub-project and named it ApplicationUser
- change it to public and inherent IdentityUser
- Create user data to be added in ApplicationDbContext to generate new columns in database
- Inside ApplicationDbContext declare a new DbSet
- Open tools NuGet console to add-migration ExtendIdentityUser, then update-database

#### Register User
- Open the Register.cshtml.cs file and change IdentityUser to ApplicationUser to enherit user register info into database and encrypt users pw

#### Create roles in database
- In program.cs, replace AddUseridentityDefault to AddIdentity and add in IdentityRole
- Declare using RoleManager and inject the rolemanager
- Open the statis file inside the sub-project Ultilities and create constants
- In Register.cshtml.cs, create roles:
- In the OnGetAsyn method populate the user data created in ApplicationUser
- In the sub-project .Ultilities, create a new class EmailSender and impletement code to handle the user email send to database
- In the EmailSender file, install .Identity.UI.Services, and impletement interface for IEmailSender
- Register the Email services in program.cs, makure add the using directive statement

#### Assign Roles in Register to allow user to select a role
- Create a dropdown to allow user select a role in Register.cshtml.cs
- Impletement log for List Role in Register.cshtml.cs
- Add a dropdown view in Register.cshtml to display the List role
- Impletement  the logic to select a single role at a time in Register.cshtml.cs
- Add in the AddDefaultTokenProviders() in program.cs

### Company CRUD operations
- Create new controller and model for company operation
- Use same format with Product CRUD
- Create a new class company model in .Model
- Create a DbSet companies in ApplicationDbContext
- Add migration addCommpanyTable and update database
- Add in a dropdown company selection
- Restrict user roles to see the company selection, unless user is company


### Shopping Cart
- Create a new model Shopping cart in .Model sub-project
- Define Product foreign keys and table 
- Create DbSet in Db application and add migration and update database
- enable auto tracking update to database to false

### Shopping cart UI
- create shopping cart controller and index page
- create a a Shoppingcart view model in Viewmodel

### Order Info
- Create 2 models class inside .Model for orderHeader and Detail, define properties and create DbSet in database, add migration and update database
- Create Repo and IRepo patterns and IUnitOfWork and UnitOfWork
- Create the view for the order detail and shipping info and access the properties inside OrderHeader and OrderDetail
#### Payment handling
- Create constants in StaticDetails file
- create Http post method for Summary and bind the view model ShoppingCartVM

- 
#### Final part: Order management
- Create an order controller
- Create an API call region (copied from Product controller)
- Create an Order view model
- Crate status order and apply filter
- Create Order history view for customer and order management for admin
- Handle order approved/pending/shipped status
- Styling consistency throughout the web
- Modify the nav bar responsive

#### Deployment Procedure
- Create Azure sql resource
- Setup server and service for sql
- Obtain connection string 
- Create a appsettings.Production.json file from the main project
- Put the connection string and the Stripe keys in the Production.json file
- Setup the hosting plan and usages on Azure
- Go to Solution explorer - publish- select Azure

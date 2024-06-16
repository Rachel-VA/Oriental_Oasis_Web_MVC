
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
   dotnet dev-certs https --trust then restart the computer

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

- Create Db property for Category in ApplicationDbContext.cs file
- Go to Tools - NuGet Manager Console to migrate it by enter the command ' add-migration AddCategoryTableToDb' and press enter. It will auto generate a migration folder and files in the project
- Go to NuGet Manager Console to 'update-database' 
- To check for update migration in database, right click dbo._EFMigrationHis and select top 1000 to see_

### Add Category Controller

- Right click on controller folder - add - controller - select an empty controller and name it Category. The name must has the controller in it
- Right click on the view folder - add - folder - name it Category (must be same name)
- Right click on the category folder - add - view - select empty razor view. It will generate a index.cshtml file

### Add Category Link in Header

- Go to Shared folder and open _Layout.cshtml and add in another nav tab and name it category

### Seed Category Table

- Go to Data folder - ApplicationDbContext.cs open it and create code to override the default func that the model generated
- Open Console NuGet to 'add-migration SeedCategoryTable' to push the created data into database
- Then ran the command 'update-database' to update the database to see changes
- To see the changes, go to database, right click on bdo.Category table

### Category CRUD operation is almost done

### Repository Pattern for database
- Create lib classes as new projects: OrientalOasis.DataAccess, OrientalOasis.Model, and OrientalOasis.Utilities
- Move Data and Migration folders into .DataAccess project, move Models folder into .Model, create a new class inside .Utilities project to store static data  

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

#### List of features in Product CRUD operations
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


### Next Features
- Company CRUD operations
- Shopping Cart
- User register

#### Later
- Order management
- Deployment

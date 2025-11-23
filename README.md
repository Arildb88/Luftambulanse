**About our Project:** <br>
We have made an ASP.NET Core MVC Aplication which is going to be used by Norsk Luftambulanse and Kartverket to collect data about unregistrered obstacles in their navigation map for Helicopters.
The application allows users to registrer, view and manage information about obstacles in a structured and user friendly way.

We have used .NET 9, Razor Views, Dependency Injection, NuGet packages (Microsoft.EntityFrameworkCore.Design, MicrosoftFrameworkCore.Tools and Pomelo.EntityFrameworkCore.MySql) and MariaDB/MySQL for the database.

**Expecations and pre requirements**
We expect the user to already have some technical knowledge and that Docker Desktop, SDK.9 and MariaDB preinstalled on their computer.

**Different versions** <br>
On main project and test project, updates that 



**Migrations:** <br>
We have deletet our Migration folder due to a namechange in our DbContext file that resultet in an error with previous migrations. We tried to change the name locally in each file, but the error presisted and we decided to delete our files and start with a clean migration history.

**How to get started Windows/MacOS:** <br>
Clone the repository:<br>
1. Open your terminal or command prompt (Git Bash, Powershell, Terminal etc.)
2. Navigate to the directory where you want to clone the repository.
3. Enter the command:<br>
git clone https://github.com/Arildb88/Luftambulanse.git <br>
cd Luftambulanse (to enter the folder of the project) <br>
4. Run docker compose file in terminal (Git Bash, Powershell, Terminal etc.):
Enter the command: <br>
docker compose up -d (Runs the docker compose file that builds the database)<br>
dotnet ef database update --project project (Updates the database to the project)<br>
5. Run application: <br>
Enter the command:<br>
dotnet watch run --project project (to start the application and open your web browser with the project launched)

**How to use the application:**<br>
You are now ready to use the application.<br>
Our first page is the Loginpage, you can either Login with the users made in Program.cs, all users have the same password Test123! .
You can also register a new user with your own email/password. In the register page you can select the "Pilot" role to instantly become a Pilot with its authorization and views. If you want to register as a different role you need to leave the field untouched or select "--choose role--" to have "no role". Then a Admin user can change your role in the system to your specific role (Caseworker, CaseworkerAdm og Admin).<br>
**Pilot:** <br>
Your homepage is the map where you can instantly start to use our application.
The map is interactive with zoom in/out, polyline marker, marker and compass needle (find my location/tracking).
You can also switch different maps and activate darkmode.
Once you place a marker on the map (if you don’t place a marker the report will use your map center as long/lat) you can press the Report button to open the report page.
The report page has pre-filled most of the information we need, but you must choose what kind of obstacle you want to report. The other details are voluntary, but for best results fill out the form to your best ability.
You can either save your report as a draft (can be edited later) or submit the report into the system for further processing.
In your Reports page you can actively follow the status on your reports as its processed by caseworkers. If your report is Rejected, you will get a message from the Caseworker as to why the report got rejected.
In the FullMap page you can see all the reports (from all pilots) in the database pinned on the map (to easily see where there might be obstacles that’s not in the Pilots navigation map yet)
The FAQ page includes frequently asked questions and answers.
If you click on your email (top right corner) you can either log out or go to Manage profile page where you can edit your profile, change password etc.<br>
**Caseworker:**<br>
You login and your homepage is the ReportsInbox where you can see all the reports in the database.
All reports have a Status so its easy to see which report you need to assign to yourself. Assign cases to yourself with the "Take this case" button and you are automatically taken to your assigned cases for further processing of the reports.
If you click the View details button you get access to the report details, there you can either Approve or Reject the Report. If you Reject the report you need to write in the Reason field to make the Reject button clickable. <br>
**CaseworkerAdmin:**<br>
You login and your homepage is the Report Inbox where you can see all the reports in the database.
In the Actions field you can Assign the case to a Caseworker user of your choice. you can Reassign the case to another Caseworker user or Unassign a Caseworker from a certain case.
CaseworkerAdm is also able to AssignCase to themself so that they can work on cases aswell ass Caseworkers.<br>
**Admin:**<br>
You login and your homepage is the AdminPage where you can change UserRoles on users in your database. Once you choose a role from the dropdown menu the changes happen instantly and the user automatically has the role. Admin user can also delete users from the database (Admin cannot delete the last Admin user).

**NEEDSTOBEUPDATED!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!**<br>
**Project Structure:**<br>
The project is structured in a way that follows the MVC (Model-View-Controller) pattern.<br>
The main folders are:<br>
- Controllers: Contains the controllers that handle the requests and responses.
- Models: Contains the models that represent the data and business logic.
- Views: Contains the Razor views that render the HTML for the user interface.
- wwwroot: Contains static files like CSS, JavaScript, and images.
- Data: Contains the database context and migration files.
- Migrations: Contains Entity Framework Core migration files for database schema changes.
- Program.cs: Main entry point for the application.
- README.md: This file, containing information about the project.
- .gitignore: Specifies files and directories to be ignored by Git to make merging branches seamless.
	

		

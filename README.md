**About our Project:** <br>
We have made an ASP.NET Core MVC Aplication which is going to be used by Norsk Luftambulanse and Kartverket to collect data about unregistrered obstacles in their navigation map for Helicopters.
The application allows users to registrer, view and manage information about obstacles in a structured and user friendly way.

We have used .NET 9, Razor Views, Dependency Injection, NuGet packages (Microsoft.EntityFrameworkCore.Design, MicrosoftFrameworkCore.Tools and Pomelo.EntityFrameworkCore.MySql) and MariaDB/MySQL for the database.

We expect the user to already have some technical knowledge and that Docker Desktop, SDK.9 and MariaDB preinstalled on their computer.

**Different versions** <br>
On main project and test project, updates that 



**Migrations:** <br>
We have deletet our Migration folder due to a namechange in our DbContext file that resultet in an error with previous migrations. We tried to change the name locally in each file, but the error presisted and we decided to delete our files and start with a clean migration history.

**How to get started:** <br>
Clone the repository:<br>
1. Open your terminal or command prompt (Git Bash, Powershell etc.)
2. Navigate to the directory where you want to clone the repository.
3. Enter the command:<br>
git clone https://github.com/Arildb88/Luftambulanse.git <br>
cd Luftambulanse (to enter the folder of the project) <br>
4. Run docker compose file in terminal (Git Bash, Powershell etc.):
Enter the command: <br>
docker compose up -d (Runs the docker compose file that builds the database)<br>
dotnet ef database update --project project (Updates the database to the project)<br>
5. Run application: <br>
Enter the command:<br>
dotnet watch run --project project (to start the application and open your web browser with the project launched)

**NEEDSTOBEUPDATED!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!**<br>
**How to use the application:**<br>
You are now ready to use the application.<br>
Our firstpage is the Loginpage, you can either Login with the users made in Program.cs, all users have the same password Test123! .
You can also register a new user with your own email/password. In the register page you can select the "Pilot" role to instantly become a Pilot with its authorization and views. If you want to register as a different role you need to leave the field untouched or select "--choose role--" to have "no role". Then a Admin user can changes your role in the system to your specific role (Caseworker, CaseworkerAdm og Admin).<br>
**Pilot:** <br>
Your homepage is the map where you can instantly start to use our application.
The map is interactive with zoom in/out, polyline marker, marker and a compass needle (find my location/tracking).
You can also switch different maps and activate darkmode.
Once you place a marker on the map (if you dont place a marker the report will use your map center as long/lat) you can press the Report button to open the report page.
The report page has pre-filled most of the information we need, but you must choose what kind of obstacle you want to report. The other details are vaulentary, but for best results fill out the form to your best ability.
You can either save your report as a draft (can edit later) or submit the report into the system for further processing.
In your Reports page you can actively follow the status on your reports as its processed by caseworkers. If your report is Rejected you will get a message from the Caseworker as to why the report got rejected.
In the FullMap page you can see all the reports (from all pilots) in the database pinned on the map (to easily see where there might be obstacles thats not in the Pilots navigation map yet)
The FAQ page includes frequently asked questions and answers.
If you click on your email (top right corner) you can either log out or go to Manage profile page where you can edit your profile, change password etc.<br>
**Caseworker:**<br>
You login and your homepage is the ReportsInbox where you can see all the report in the database.
All reports have a Status so its easy to see which report you need to assign to yourself. 




You are prompted with a view of a map where you can:<br>
- Zoom in and out
- Choose between 4 different map types
- Report Obstacle button (does not work at the moment)
- The red pin allowes our app to track your current position (allow tracking when prompted) and continues to track your position as you move
	- you can turn this off by clicking the pin or move the map
	- We are going to add a feature where you can click on the map to add a marker, that you can use as coordinates for when you report an obstacle (this function works in the Reports page)
- The menu bar at the top allowes you to navigate to different pages (this will be modified and changes in the future)
	- Home (the map page)
	- Privacy (a page with privacy information)
	- FAQ (a page with frequently asked questions)
	- Map (was made to show the map, but is now merged with Home, Map will be deleted from the Menu)
	- Log In (a page where you can login, this will be modified in the future)
	- Reports (a page where you can see all the obstacles that has been reported, and you can add a new report)
	- Administrator (a page where you can manage users, site settings and more, this page is under development)
	- About (a page with information about the project and the team behind it)

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
	

		

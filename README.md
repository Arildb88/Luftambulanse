We have made an ASP.NET Core MVC Aplication which is going to be used by Norsk Luftambulanse and Kartverket to collect data about unregistrered obstacles in their navigation map for Helicopters.
The application allows users to registrer, view and manage information about obstacles in a structured and user friendly way.

We have used .NET 9, Razor Views, Dependency Injection and MariaDB/MySQL for the database.

How to get started:

1. Clone the repository:
	1. Open your terminal or command prompt (Git Bash, Powershell etc.)
	2. Navigate to the directory where you want to clone the repository.
	3. Enter the command:
		4. git clone https://github.com/Arildb88/Luftambulanse.git
		5. cd Luftambulanse (to enter the folder of the project)
	6. Restore packages:
		7. dotnet watch run (to start the application and open your web browser with the project launched)

You are now ready to use the application.
We will add a login screen where you have to login with a username and password (BankID or similar)
You are prompted with a view of a map where you can:
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
	
	

		

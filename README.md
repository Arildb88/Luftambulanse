# Gruppe 4 - NLA Prosjekt

## About the Project
The project is made in collaboration between student group 4, Norsk Luftambulanse and Kartverket. <br>
This project seeks to solve the problem of unregistered aviation obstacles that pose a danger for pilots during emergency responses. Kartverket also need a solution that handles and processes these obstacle reports. <br>

The application allows users to register, view and manage information about obstacles in a structured and user friendly way. Pilots can register obstacles on a map, for caseworkers at Kartverket to process and validate these reports. 
The system has different roles that restrict or gives privileges, these can be altered by the admin user. <br>

---
# Table of Contents
1. [About the Project](#about-the-project)
2. [Getting Started (Windows/MacOS)](#how-to-get-started-windowsmacos)
3. [How to Use the Application](#how-to-use-the-application)
    - [Pilot](#pilot)
    - [Caseworker](#caseworker)
    - [Caseworker Admin](#caseworker-admin)
    - [Admin](#admin)
4. [User Roles & Permissions](#user-roles--permissions)
5. [Technology & Tools](#technology--tools)
6. [Project Structure](#project-structure)
7. [Security Features](#security-features)
8. [Testing](#testing)
    - [Peer to Peer testing]("how-to-run-tests)
9. [Team](#team)
10. [Additional Notes](#additional-notes)
---

## How to get started Windows/MacOS
**Expectations:** Some prior technical knowledge. <br> 
**Requirements:** Preinstall Docker Desktop, SDK.9 and MariaDB on their computer. <br>

Clone the repository: <br>
1. Open your terminal or command prompt (Git Bash, Powershell, Terminal etc.) <br>
2. Navigate to the directory where you want to clone the repository. <br>
3. Enter the command:<br>
```git clone https://github.com/Arildb88/Luftambulanse.git``` <br>
```cd Luftambulanse``` (to enter the folder of the project). <br>
4. Run docker compose file in terminal (Git Bash, Powershell, Terminal etc.) <br>
Enter the command: <br>
```docker compose up -d``` (Runs the docker compose file that builds the database)<br>
```dotnet ef database update --project project``` (Updates the database with migrations)<br>
5. Run application: <br>
Enter the command: <br>
```dotnet watch run --project project``` (to start the application and open your web browser with the project launched) <br>
6. To run the tests enter the command:<br> 
```dotnet test``` <br>


## How to Use the Application
When launching the application, the first page displayed is the **Login** page.

### Login Options
You may log in using the predefined users:

| Email | Role | Password |
|-------|-------|----------|
| pilot@test.com | Pilot | Test123! |
| caseworker@test.com | Caseworker | Test123! |
| caseworkeradm@test.com | CaseworkerAdm | Test123! |
| admin@test.com | Admin | Test123! |

You may also register a new user.  
Selecting **Pilot** during registration grants pilot access immediately.  
Leaving the role field empty will create a user without a role — an Admin can later assign one.

### Pilot
- Your homepage is an interactive map (zoom, markers, dark mode, compass needle, location tracking).
- Place a marker to report an obstacle. If no marker is placed, the map center coordinates will be used.
- The report page auto-fills most required fields; obstacle type must be selected manually.
- Reports can be saved as drafts or submitted for processing.
- **My Reports** displays the processing status of your submitted reports.
- **FullMap** shows all reports from all pilots.
- Manage your profile or change your password via the top-right user menu.

### Caseworker
- Homepage: **Reports Inbox** with all submitted reports.
- Assign a report to yourself using *Take this case*.
- Open report details to approve or reject a submission.
- Rejecting a report requires entering a reason in the provided field.

### Caseworker Admin
- Has access to assign, reassign, or unassign reports for any Caseworker.
- Can assign cases to themselves and process them the same way a Caseworker would.

### Admin
- Homepage: **Admin Page**, used to manage user roles.
- Changing a user’s role applies immediately.
- Admins may delete users, except the final remaining Admin account.

### User Roles & Permissions
|Role|Permissions|
|-----|-----------|
|Pilot|				Create and submit obstacle reports|
|Caseworker|		Review, update and process obstacle reports|
|CaseworkerAdm|		Assign reports to caseworkers, review, update and process obstacle reports|
|Admin|				Full access and role/user administration|


## Technology & Tools
**Technologies Used:**
* JavaScript, C#, HTML & CSS.
* .Net 9, MVC, Razor Views.
* MariaDB/MySQL.
	* MariaDB database with ASP.NET Identity.
* Docker & Docker Compose.
* Leaflet.js for GIS map functionality.
* NuGet:
    * Microsoft.EntityFrameworkCore.Design
    * Microsoft.EntityFrameworkCore.Tools
    * Pomelo.EntityFrameworkCore.MySql

**The system consists of:**
* Webapplication  in ASP.NET Core 9 with MVC/Razor Views.
* Both applications are running in a Docker-container.
* Leaflet.js for map interaction.
* Authorization and authentication is based on roles (Pilot, Caseworker, CaseworkerAdm and Admin).

```
┌────────────────────────────┐
│ Pilot / Admin / Caseworker │
│   (User Interface)         │
└───────────────┬────────────┘
                │ HTTP/HTTPS
┌───────────────▼────────────┐
│ ASP.NET Web Application    │
│ • MVC / Razor Views        │
│ • Authentication/Identity  │
│ • EF Core        			 │
│ • REST endpoints           │
└───────────────┬────────────┘
                │ TCP 3306
┌───────────────▼────────────┐
│ MariaDB Database           │
│ • Identity tables          │
│ • Obstacle reports         │
│ • Role & user assignments  │
└────────────────────────────┘
```

Docker Compose connects the containers in a shared network.

## Project Structure
**MVC-Model** <br>
The project's structure follows the MVC (Model-View-Controller) pattern, with templates from ASP.NET web application.<br>
The main folders are:<br>
- Controllers: Contains the controllers that handle the requests and responses.
- Models: Contains the models that represent the data and business logic.
- Views: Contains the Razor views that render the HTML for the user interface.
- Areas: Contains files from the ASP.CORE Identity . Handles user, role, login & password with predefined views and templets (CSHTML).
- wwwroot: Contains static files like CSS, JavaScript, and images.
- Data: Contains the database context and migration files.
- Migrations: Contains Entity Framework Core migration files for database schema changes.
- Program.cs: Main entry point for the application.
- README.md: This file, containing information about the project.
- .gitignore: Specifies files and directories to be ignored by Git to make merging branches seamless.
	
|Folder|Contents|
|------|--------|
|/Areas|Identity Framework core & pre defined pages with Identity Framework core functions|
|/Controllers|            MVC controllers|
|/Models|                 Domain models + Identity models|
|/Views|                  Razor pages for UI|
|/Data|                   DbContext, database configuration|
|/wwwroot|                Styles, scripts, and Leaflet map logic|
|/Project.UnitTests|      Unit tests|
|/.gitignore|             Specifies files and directories to be ignored by Git to make merging branches seamless|
|/Program.cs|             Main entry point for the application|
|/Readme|                 Containing information about the project|


## Security Features
Implemented security:
* ASP.NET Identity authentication with hashed passwords.
* Role-based authorization (Pilot, Caseworker, CaseworkerAdm and Admin).
* Server-side validation of all operations.
* DB user permissions restricted (no root access from app).
* Anti-forgery protection (CSRF).


## Testing
* Unit tests for models and controllers.
* System testing: login, report submission, role-based access.
* Security testing: unauthorized access attempts, validation.
* Usability testing: pilot user feedback (from Q&A and peer to peer testing).
* Query tests: SQL script and queries to test data validity.

### Peer to peer testing
The group has had multiple family members and friends test our solution to evaluate whether the application is easy to understand and use. Based on this feedback, we have made several UI improvements with a strong focus on accessibility and a self-explanatory user interface. The results from our testers show that they are able to use the application (as a Pilot user) without needing much guidance.

**How To Run Tests** <br>
When in current directory (your/path/luftambulanse), run the command: <br>
```dotnet test``` <br>

## Team
|Name|                     Role|
|----|-------------------------|
|Arild Bjørnetrø|          Developer|
|David Rakic|              Developer|
|Einar Alme|               Developer|
|Fahrtin Assenov|          Developer|
|Hossein Akbar|            Developer|
|Jonas Bendal|             Developer|


## Additional Notes
**Migrations** <br>
We have deleted our Migration folder due to a namechange in our DbContext file that resultet in an error with previous migrations. We tried to change the name locally in each file, but the error persisted and we decided to delete our files and start with a clean migration history.



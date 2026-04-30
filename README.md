 Cloud Infrastructure Provisioning Portal 

An internal self-service portal built with ASP.NET Core MVC. 

The goal of this project is to stop developers from having to email IT every time they need a new test server. Instead, they can log into this dashboard, choose their RAM and OS, check off the software they need (like Docker or Node.js), and hit submit. IT gets a clean queue of requests to approve or reject.

Right now, we are entirely focused on the "Model" part of MVC. There is no frontend UI or backend routing yet. 

This repository currently contains the foundational database blueprint using C# Entity classes and Entity Framework Core conventions. We've mapped out the exact tables, relationships, and validation rules the app will need when we build the interface in the next phase.

The Database Structure

We've kept the schema clean and realistic. Here are the core models living in the `Models/` folder:

* **`Developer`**: The user making the request.
* **`ServerInstance`**: The actual VM being requested. We're enforcing strict validation here (e.g., RAM is capped between 1-128GB, and hostnames must follow Linux naming conventions). It also uses an `InstanceSize` enum for hardware tiering.
* **`SoftwarePackage`**: The catalog of available pre-installed tools.
* **`ServerSoftware`**: The junction table. Since one server can have many software packages, and a package can be on many servers, this handles our many-to-many relationship.

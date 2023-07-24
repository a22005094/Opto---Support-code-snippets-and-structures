# [DEISI343] Opto - Elasticsearch communication module

This is a secondary project for DEISI343 Final Course Project's main Unity project, Opto.
With the exception of the "Extra deliverables" folder on the project root, all developments are located inside the "Assets" directory, namely the Models and Scripts folders.
Three goals are intended here:

1. Implement a communication module to report treatment results, and test the whole Back-end service application stack(Nginx, Elasticsearch, Kibana and dashboards);
2. Data generation and management structures;
3. Other helpful snippets;
4. Provide other important deliverables.

## 1 - Data communication module
Developments were made for a module to support communications to an Elasticsearch server, in regards to pushing treatment metrics belonging 
to patient Sessions and Exercises performed during each Session.

To abstract API REST endpoint connections to Elasticsearch as simple method invocations, an official library endorsed by Elastic was used, named NEST.
This library provides out-of-the-box documentation, strongly-typed data structures that map to Elasticsearch's requests and responses, auto-mapping of 
a C# class fields to the respective Elasticsearch index fields (allowing the smooth creation of Indices), and others.
Dependencies on NEST .NET client are placed on the "Extra deliverables" folder, on the project's root directory.

Both development and production environments were successful in pushing new data to Elasticsearch server (through proper configuration of Elasticsearch client connection settings, depending on the environment's information, e.g.: IP Address, Username and Password, etc).


## 2 - Data management structures
During development phase of the project, gathering sample test data was essential to validate expected results for use cases regarding patient results when finishing Treatment sessions, and analyzing results evolution on Kibana dashboards. For this reason, some code samples were developed to generate random Session and Exercises data.
This code has the ability to detect if a local JSON file exists on the device, containing the patient's UserID (or generating a new ID otherwise), in order to associate future data generated to the same User, resembling the expected functionality from the main Unity project as both parts are futurely merged.

Two additional data structures were also implemented as Singleton objects (folder Assets/Models/Managers): one to abstract communications to Elasticsearch (ESManager), and the other to simplify the entire data management process (DataManager), both to be used on a later version of the main Unity project, as means to easily integrate the entire communications module and abstract its complexity into a small set of simple function calls.


## 3 - Other helpful snippets
Other code experiments have also been provided here, such as a snippet for local device I/O file operations, namely for persisting and loading JSON files on a mobile phone running a Unity application (likely to be used on future updates of the Opto application).

## 4 - Other important deliverables
On the root folder of the project, an "Extra deliverables" folder was created containing some of the most important items necessary during the implementation of the entire back end infrastructure (Elasticsearch, Kibana, Nginx, Amazon Web Services helping tools).

Some of these are:
* Elasticsearch: configuration file (.yml), ingest pipelines implemented, Let's Encrypt certificates for HTTPS, authentication credentials (and more...).
* Kibana: configuration file (.yml), enrollment token to integrate Kibana with Elasticsearch, and some commands with notes (in Portuguese) taken during initial setup.
* Nginx: configuration file for Web server and Reverse proxy, and final prototype of a dashboard HTML shipped to production (fully working with new data sent to Elasticsearch in production).
* Other deliverables, such as DLL dependencies for NEST .NET Elasticsearch client for a C# project, PCSP commands for file transfer between and to AWS virtual machines, Postman collection of test API calls made to Elasticsearch during development (and more...).


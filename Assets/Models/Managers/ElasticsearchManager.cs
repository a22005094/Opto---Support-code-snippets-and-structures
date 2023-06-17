using Nest;
using System;
using System.Threading.Tasks;

namespace Assets.Models.Managers
{

    #region (INFORMATION ABOUT THIS CLIENT)
    //
    //  - This Class is responsible for establishing connections to the Elasticsearch server, and managing
    //    CRUD operations over generated application data, by abstracting Elasticsearch API access into simpler method calls.
    //
    //  - This level of abstraction is achieved by using NEST, a .NET Package developed by Elastic
    //    that wraps the REST API calls into a familiar "remote method calls using Objects" fashion, providing easier API integration,
    //    strongly-typed requests and responses, while also providing side documentation about most Types and Method calls shipped with the package.
    //
    //  - Behind the curtains, NEST client uses another Elastic client as its core, 'Elasticsearch.Net' (also available for use),
    //    but works as a low-level client that only provides a limited set of API Types mapping, little to no documentation and,
    //    overall, offering limited functionality when compared to NEST, translating into a (much) steeper learning curve.
    //    After a brief moment of experimentation between both, it was found that NEST was the best way to go here.
    //    (See: "Why two clients?" - https://www.elastic.co/guide/en/elasticsearch/client/net-api/7.17/introduction.html#_why_two_clients)
    //    [IMPORTANT]: It should be noted that starting from Elasticsearch v8.0, both of these .NET clients are set to merge into a new, single .NET client,
    //                 but since its development is still underway at an early phase and NEST is still fully compatible, a conscious decision was made to
    //                 still keep NEST for being feature-rich and having good documentation, which immensely helped the development phase.
    //
    //  - During development, some additional configurations are also being done during NEST client initialization, to help with
    //    debugging, and assess expected functionality from the service during Client-Server API calls.
    //
    //  - For more information about NEST Client, you may consult the official documentation from Elastic:
    //    https://www.elastic.co/guide/en/elasticsearch/client/net-api/7.17/nest.html
    //
    //
    //  - As a final note, this client implementation tries to closely resemble the Singleton software design pattern, to ensure that only a single
    //    instance of the Elasticsearch client is actually initialized and used throughout the application execution time, regardless of how many times
    //    the client is obtained and used to while interacting with the application.
    //    It makes use of lock (concurrency) concepts to ensure Thread Safety during initialization.
    //    Two important links are related to this topic, mainly (1)* why this decision was made, and (2)** reference links for Singleton implementation.
    //      > (1)*  It's a good practice recommended by official documentation (The page refers to another Client, but can be adapted to NEST)
    //              https://www.elastic.co/guide/en/elasticsearch/client/net-api/current/recommendations.html#_reuse_the_same_client_instance
    //      > (2)** https://www.tutorialsteacher.com/csharp/singleton // https://forum.unity.com/threads/singleton-vs-static.197169/
    //
    // -- [USEFUL LINKS] -------------------------------------------------------------------------------------------------------------
    //   Below is a list with some of the main links (out of many others that were also helpful), used to get started
    //   with Elasticsearch (Setup & API use). The official documentation was generally sufficient, with only a few unofficial sources
    //   being used for more specific requirements.
    //
    //  *** Elasticsearch install reference (Ubuntu) ***
    //   |https://www.elastic.co/guide/en/elasticsearch/reference/8.6/install-elasticsearch.html
    //    |-- https://www.elastic.co/guide/en/elasticsearch/reference/8.6/deb.html
    //    |   |--- https://www.elastic.co/guide/en/elasticsearch/reference/8.6/deb.html#install-deb
    //
    //  *** Some Elasticsearch concepts ***
    //   > (!) How ElasticSearch data insertion works - Indexes and Documents: https://www.elastic.co/guide/en/elasticsearch/reference/8.0/documents-indices.html
    //   > (!) Importing a self-signed test certificate, to the Trusted Root Certification Authorities store (for Windows operating system)
    //     -> https://jaryl-lan.blogspot.com/2022/03/write-to-elasticsearch-with-serilog-in.html
    //   > (!) Enabling remote connections: https://stackoverflow.com/questions/33696944/how-do-i-enable-remote-access-request-in-elasticsearch-2-0
    //   > Elasticsearch overview: https://www.elastic.co/guide/en/elasticsearch/reference/8.0/getting-started.html
    //
    //  *** API Reference & Examples ***
    //   > An approach to Elasticsearch API, using REST (example, via Postman): https://blog.adnansiddiqi.me/getting-started-with-elasticsearch-7-in-python/
    //   > Understanding how NEST client works:
    //     i) An example of indexing documents using NEST (in other words, inserting data in Elasticsearch)
    //        -> https://www.elastic.co/guide/en/elasticsearch/client/net-api/7.17/indexing-documents.html
    //     ii) Good example of CRUD operations using NEST:
    //        -> https://yamannasser.medium.com/simplifying-elasticsearch-crud-with-net-core-a-step-by-step-guide-25c86a12ae15
    //     iii) Some more examples, but using the low-level Elasticsearch.NET client (yet, still relevant):
    //        -> https://www.elastic.co/guide/en/elasticsearch/client/net-api/current/examples.html
    // -------------------------------------------------------------------------------------------------------------------------------
    #endregion


    public sealed class ElasticsearchManager
    {
        private static ElasticsearchManager _instance = null;
        private static ElasticClient _elasticClientInstance = null;
        private static readonly object _lockObj = new object();

        private const string EXERCISES_INDEX = "exercises";      // the name of the Index where exercise data is stored
        private const string SESSIONS_INDEX = "sessions";        // the name of the Index where session  data is stored


        // Private constructor ensures this Class cannot be instantiated outside of its own scope
        private ElasticsearchManager() { }

        // * The getter of this Singleton instance.
        public static ElasticsearchManager Instance
        {
            get
            {
                lock (_lockObj)
                {
                    if (_instance == null)
                    {
                        // ------------------------------------------------------------
                        //   Elasticsearch NEST client configuration & initialization
                        // ------------------------------------------------------------
                        try
                        {
                            // Endpoint URI
                            // (v1) Uri uriServidor = new Uri("https://192.168.64.131:9200");
                            // (v2) Uri uriServidor = new Uri("http://35.83.233.168/elasticsearch");
                            Uri uriServer = new Uri("https://www.deisi343.pt/elasticsearch");
                            var clientSettings = new ConnectionSettings(uriServer);

                            // ** DEPRECATED **
                            // X509 Cert fingerprint (currently using Elasticsearch auto-generated certificate)
                            // var x509cert = new X509Certificate2(File.ReadAllBytes(@"C:\http_ca.crt"));

                            // Certificate Fingerprint & Authentication
                            //clientSettings.ClientCertificate(x509cert);
                            clientSettings.BasicAuthentication("elastic", "ekFV-epAKGu7OssZPKzK");

                            // *** DEPRECATED - The Index to work on will depend on the context of the API call! ***
                            // Default Index to point to (it can always be changed during server requests)
                            //clientSettings.DefaultIndex("exercises");

                            // - Per the official documentation, to maximize compatibility between NEST client [v7.17] and the
                            //    latest Elasticsearch versions (as of now, [v8.7.x]), it is recommended to use this configuration
                            //    durint client initialization, which enables Compatibility Mode.
                            //    (See: https://www.elastic.co/guide/en/elasticsearch/client/net-api/7.17/connecting-to-elasticsearch-v8.html#enabling-compatibility-mode)
                            // - However, brief investigation has shown some users have had issues before when using this configuration.
                            //    For that reason, this line is EXPERIMENTAL.
                            clientSettings.EnableApiVersioningHeader();

                            // ** Experimental Configs **
                            clientSettings
                                // Good for development purposes, eg. prints more detailed stack traces
                                .EnableDebugMode()
                                // Pretty-format responses, in case of being necessary to read in their original format
                                .PrettyJson()
                                // Don't infer ID field from used POCO classes
                                .DefaultDisableIdInference()
                                // Timeout requests to Elasticsearch if they exceed an "acceptable" time
                                .RequestTimeout(TimeSpan.FromSeconds(10))
                            ;

                            // ----------------------------------------------------------- //
                            // * Other possible, interesting parameters...
                            // ----------------------------------------------------------- //
                            //  -> CertificateFingerprint("")
                            //      The certificate fingerprint, can be obtained from the original certificate generated during Elasticsearch setup:
                            //      [openssl x509 -fingerprint -sha256 -in config/certs/http_ca.crt] (Credits: https://discuss.elastic.co/t/where-can-i-see-my-certificate-fingerprint/319335/3)
                            //  -> MaximumRetries
                            //  -> MaxRetryTimeout
                            //  -> OnRequestCompleted - possibly for custom logging (?)
                            //  -> ThrowExceptions
                            // ----------------------------------------------------------- //

                            // Generate a new instance of ElasticsearchClient
                            _elasticClientInstance = new ElasticClient(clientSettings);
                            _instance = new ElasticsearchManager();
                        }
                        catch (Exception)
                        {
                            // If an exception occurred, make the Instance unusable, because something went wrong.
                            _instance = null;
                        }
                    }
                }

                return _instance;
            }
        }

        // ** Manager methods **

        public async void CreateIndexFromObject<T>(string indexName, T sampleObject) where T : class
        {
            // [indexName] is the name of the Index to create - could be "exercises", "sessions", or something else.
            // [sampleObject] is used to Auto-Map the object's fields into the corresponding Elasticsearch index fields.

            // Credits for the implementation, namely, mapping <T> generics into the CreateAsync function
            // (shows that it's needed to add [where T: class] at method header):
            // |-> https://stackoverflow.com/questions/52963404/c-sharp-nest-elastic-generic-index-create

            try
            {
                if (_instance == null)
                    throw new NullReferenceException("ERROR: Elasticsearch client has not been properly initialized.");

                var response = await _elasticClientInstance.Indices.CreateAsync(indexName, selector => selector.Map<T>(m => m.AutoMap()));

                if (response.IsValid)
                {
                    Console.WriteLine($"Successfully created Index '{indexName}'!");
                }
                else
                {
                    Console.WriteLine($"Oops.. failed to create Index '{indexName}'!");
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine("EXCEPTION: [" + exc.Message + "], StackTrace: [" + exc.StackTrace + "]");
            }
        }

        public async Task<bool> Push_Exercise_To_Elasticsearch(Exercise exercise)
        {
            // Responsible for pushing an Exercise to Elasticsearch server.
            // It points to the 'exercises' Index, sending finished exercise data to the server (accessible @ currentExercise variable).
            // Returns a boolean, indicating if the operation was successful.

            IndexResponse response = await _elasticClientInstance.IndexAsync(exercise, selector => selector.Index(EXERCISES_INDEX));

            if (response.IsValid)
            {
                Console.WriteLine($"Index Exercise OK - Result ID: [{response.Id}]");
            }
            else
            {
                Console.WriteLine("error");

                // var debugInfo = response.DebugInformation;
                // var error = response.ElasticsearchServerError?.Error;
            }

            return response.IsValid;
        }

        public async Task<bool> Push_Session_To_Elasticsearch(Session session)
        {
            // Responsible for pushing a Session to Elasticsearch server.
            // It points to the 'sessions' Index, sending finished session data to the server (accessible @ currentSession variable).
            // Returns a boolean, indicating if the operation was successful.

            IndexResponse response = await _elasticClientInstance
                .IndexAsync(session,
                    selector => selector
                        .Index(SESSIONS_INDEX)
                .Pipeline("Pipeline_Handle_Session_Metrics")
                );

            if (response.IsValid)
            {
                Console.WriteLine($"Index Session OK - Result ID: [{response.Id}]");
            }
            else
            {
                Console.WriteLine("error");

                // var debugInfo = response.DebugInformation;
                // var error = response.ElasticsearchServerError?.Error;
            }

            return response.IsValid;
        }


    }
}

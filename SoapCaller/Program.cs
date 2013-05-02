using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Xml;
using System.Xml.Linq;

using enterprise = SoapCaller.SfdcReference;
using partner = SoapCaller.SfdcPartnerReference;

namespace SoapCaller
{
    class Program
    {
        private static string sessionId = string.Empty;
        private static string serverUrl = string.Empty;

        static void Main(string[] args)
        {
            //print message to console
            Console.WriteLine(" :: DeveloperForce Demo :: ");

            //login to the SOAP Enterprise endpoint
            AuthenticateSfdcEnterpriseUser();

            //login to the SOAP Partner endpoint
            //AuthenticateSfdcPartnerUser();

            //query account records via Enterprise API
            QueryEnterpriseRecord();

            //create new records via the Enterprise API
            //CreateEnterpriseRecords();

            //query account records via the Partner API
            //QueryPartnerRecords();

            //create new records via the Partner API
            //CreatePartnerRecords();
        }


        /// <summary>
        /// Call SFDC endpoint and retrieve authentication token and API URL for SOAP callers
        /// </summary>
        private static void AuthenticateSfdcEnterpriseUser()
        {
            //print message to console
            Console.WriteLine("Authenticating against the Enterprise API ...");

            //use default binding and address from app.config
            using (enterprise.SoapClient loginClient = new enterprise.SoapClient("Soap"))
            {
                //set account password and account token variables
                string sfdcPassword = "[password]";
                string sfdcToken = "[token]";

                //set to Force.com user account that has API access enabled
                string sfdcUserName = "[username]";

                //create login password value
                string loginPassword = sfdcPassword + sfdcToken;

                //call Login operation from Enterprise WSDL
                enterprise.LoginResult result = 
                    loginClient.login(
                    null, //LoginScopeHeader
                    sfdcUserName,
                    loginPassword);

                //get response values
                sessionId = result.sessionId;
                serverUrl = result.serverUrl;

                //print response values
                Console.WriteLine(string.Format("The session ID is {0} and server URL is {1}", sessionId, serverUrl));
                Console.WriteLine("");
                Console.WriteLine("Press [Enter] to continue ...");
                Console.ReadLine();
            }
        }

        /// <summary>
        /// Call SFDC endpoint and retrieve authentication token and API URL for SOAP callers
        /// </summary>
        private static void AuthenticateSfdcPartnerUser()
        {
            //print message to console
            Console.WriteLine("Authenticating against the Partner API ...");

            //use default binding and address from app.config
            using (partner.SoapClient loginClient = new partner.SoapClient("Soap1"))
            {
                //set account password and account token variables
                string sfdcPassword = "[password]";
                string sfdcToken = "[token]";

                //set to Force.com user account that has API access enabled
                string sfdcUserName = "[username]";

                //create login password value
                string loginPassword = sfdcPassword + sfdcToken;

                //call Login operation from Enterprise WSDL
                partner.LoginResult result =
                    loginClient.login(
                    null, //LoginScopeHeader
                    null, //calloptions
                    sfdcUserName,
                    loginPassword);

                //get response values
                sessionId = result.sessionId;
                serverUrl = result.serverUrl;

                //print response values
                Console.WriteLine(string.Format("The session ID is {0} and server URL is {1}", sessionId, serverUrl));
                Console.WriteLine("");
                Console.WriteLine("Press [Enter] to continue ...");
                Console.ReadLine();
            }
        }

        /// <summary>
        /// Use Enteprise API to query and retrieve SFDC records
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="serverUrl"></param>
        private static void QueryEnterpriseRecord()
        {
            Console.WriteLine("Querying account records with the Enterprise API ...");

            //set query endpoint to value returned by login request
            EndpointAddress apiAddr = new EndpointAddress(serverUrl);

            //instantiate session header object and set session id
            enterprise.SessionHeader header = new enterprise.SessionHeader();
            header.sessionId = sessionId;

            //create service client to call API endpoint
            using (enterprise.SoapClient queryClient = new enterprise.SoapClient("Soap", apiAddr))
            {
                //query standard or custom objects

                //create SOQL query statement
                string query = "SELECT Name, AccountNumber, BillingState FROM Account WHERE BillingState = 'CA'";

                enterprise.QueryResult result = queryClient.query(
                    header, //sessionheader
                    null, //queryoptions
                    null, //mruheader
                    null, //packageversion
                    query);

                //cast query results
                IEnumerable<enterprise.Account> accountList = result.records.Cast<enterprise.Account>();
                
                //show results
                foreach (var account in accountList)
                {
                    Console.WriteLine(string.Format("Account Name: {0}", account.Name));
                }

                Console.WriteLine("");
                Console.WriteLine("Query complete.");
                Console.ReadLine();

                //retrieve example

                //call retrieve operation to get one or more records of a given type and ID
                enterprise.sObject[] retrievedAccounts = queryClient.retrieve(
                    header, //sessionheader
                    null, //queryoptions
                    null, //mruheader
                    null, //packageversion
                    "Name, BillingState", //fieldlist
                    "Account", //objectype
                    new string[] { "001E000000N1H1O" } //record IDs
                    );

                foreach (enterprise.sObject so in retrievedAccounts)
                {
                    enterprise.Account acct = (enterprise.Account)so;
                    Console.WriteLine(string.Format("Account Name: {0}, Account State: {1}", acct.Name, acct.BillingState));
                }

                Console.WriteLine("");
                Console.WriteLine("Retrieve complete.");
                Console.ReadLine();
            }
        }

        /// <summary>
        /// Use Enterprise API to create new SFDC records
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="serverUrl"></param>
        private static void CreateEnterpriseRecords()
        {
             Console.WriteLine("Creating an account record with the Enterprise API ...");

            //set query endpoint to value returned by login request
            EndpointAddress apiAddr = new EndpointAddress(serverUrl);

            //instantiate session header object and set session id
            enterprise.SessionHeader header = new enterprise.SessionHeader();
            header.sessionId = sessionId;

            //create service client to call API endpoint
            using (enterprise.SoapClient createClient = new enterprise.SoapClient("Soap", apiAddr))
            {
                enterprise.Account newAcct = new enterprise.Account();
                newAcct.Name = "DevForce02";
                newAcct.AccountNumber = "10043332";
                //all non-string fields must have their corresponding <name>Specified property set
                newAcct.AnnualRevenue = 4000000f;
                //newAcct.AnnualRevenueSpecified = true;

                enterprise.Opportunity o = new enterprise.Opportunity();
                o.Name = "Opp2";
                o.StageName = "Prospecting";
                o.CloseDate = DateTime.Parse("2013-03-22");
                o.CloseDateSpecified = true;

                enterprise.SaveResult[] results;

                createClient.create(
                    header, //sessionheader
                    null, //assignmentruleheader
                    null, //mruheader
                    null, //allowfieldtruncationheader
                    null, //disablefeedtrackingheader
                    null, //streamingenabledheader
                    null, //allornoneheader
                    null, //debuggingheader
                    null, //packageversionheader
                    null, //emailheader
                    new enterprise.sObject[] { o }, //objects to add
                    out results //results of the creation operation
                    );

                //only added one item, so looking at first index of results object
                if (results[0].success)
                {
                    Console.WriteLine("Account successfully created.");
                }
                else
                {
                    Console.WriteLine(results[0].errors[0].message);
                }

                Console.ReadLine();
            }
        }

        /// <summary>
        /// Use Partner API to query SFDC records
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="serverUrl"></param>
        private static void QueryPartnerRecords()
        {
            Console.WriteLine("Querying account records with the Partner API ...");

            //set query endpoint to value returned by login request
            EndpointAddress apiAddr = new EndpointAddress(serverUrl);

            partner.SessionHeader header = new partner.SessionHeader();
            header.sessionId = sessionId;

            //create service client to call API endpoint
            using (partner.SoapClient queryClient = new partner.SoapClient("Soap1", apiAddr))
            {
                //create SOQL query statement
                string query = "SELECT Name, AccountNumber, BillingState FROM Account";

                partner.QueryResult result = queryClient.query(
                    header, //sessionheader
                    null, //calloptions
                    null, //queryoptions
                    null, //mruheader
                    null, //packageversionheader
                    query //query string
                    );

                //loop through results
                foreach (partner.sObject account in result.records)
                {
                    string acctName = account.Any[0].InnerText;
                    string acctNum = account.Any.First(x => x.LocalName == "AccountNumber").InnerText;
                    Console.WriteLine(string.Format("Account Name: {0}, Account Number: {1}", acctName, acctNum));

                }

                Console.WriteLine("");
                Console.WriteLine("Query complete.");
                Console.ReadLine();
            }
        }

        /// <summary>
        /// Use Partner API to create new SFDC records
        /// </summary>
        private static void CreatePartnerRecords()
        {
            Console.WriteLine("Creating an account record with the Partner API ...");

            //set query endpoint to value returned by login request
            EndpointAddress apiAddr = new EndpointAddress(serverUrl);

            partner.SessionHeader header = new partner.SessionHeader();
            header.sessionId = sessionId;

            //create service client to call API endpoint
            using (partner.SoapClient queryClient = new partner.SoapClient("Soap1", apiAddr))
            {
                partner.sObject account = new partner.sObject();
                account.type = "Account";

                //create XML containers for necessary XML document and elements
                XmlDocument rootDoc = new XmlDocument();
                XmlElement[] accountFields = new XmlElement[3];

                XElement[] test = new XElement[2];
                test[0] = new XElement("node1", "value");

                //add fields
                accountFields[0] = rootDoc.CreateElement("Name");
                accountFields[0].InnerText = "DevForce06";
                accountFields[1] = rootDoc.CreateElement("AccountNumber");
                accountFields[1].InnerText = "1004441239";
                accountFields[2] = rootDoc.CreateElement("AnnualRevenue");
                accountFields[2].InnerText = "4000000";

                //set object property to array
                account.Any = accountFields;

                partner.SaveResult[] results;

                queryClient.create(
                    header, //sessionheader
                    null,   //calloptions
                    null,   //assignmentruleheader
                    null,   //mruheader
                    null,   //allowfieldtruncationheader
                    null,   //disablefeedtrackingheader
                    null,   //streamingenabledheader
                    null,   //allornothingheader
                    null,   //debuggingheader
                    null,   //packageversionheader
                    null,   //emailheader
                    new partner.sObject[] { account }, //new accounts
                    out results //result of create operation
                    );


                //only added one item, so looking at first index of results object
                if (results[0].success)
                {
                    Console.WriteLine("Account successfully created.");
                }

                Console.ReadLine();
            }
        }
    }
}

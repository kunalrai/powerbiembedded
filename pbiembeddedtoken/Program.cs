using AppOwnsData.Models;
using AppOwnsData.Services;
using Microsoft.Identity.Client;
using Microsoft.PowerBI.Api;
using Microsoft.PowerBI.Api.Models;
using Microsoft.Rest;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pbiembeddedtoken
{
    class Program
    {
        
        static void Main(String[] args)
        {
           var result =  Token.GetToken().Result;


            Console.WriteLine($"{ result }");

        }

      
    }
}

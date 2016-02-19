using GerenciadorFtp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TesteFtp
{
    class Program
    {
        static void Main(string[] args)
        {
            FtpSolution ftp = new FtpSolution();
            //ftp.setCredentials("********", "****************", "********");
            ftp.setCredentials("*******", "********", "********");
            //Test Ok 19012016
            ftp.uploadFile(@"C:\Users\proje\Pictures\illustration_07.jpg", "httpdocs/MakeTecware/LogoTecware.jpg");
            //Test Ok 19012016
            //ftp.MakeDir("testeMake");
            //Test Ok 19012016
            //ftp.MakeDir("ftp://ftp.tecware.com.br/httpdocs/MakeTecware/testeMak/",@"C:\Users\sebastiao.martins\Documents\WPA Files\TelaTecware 1440x900.jpg","Logo_tecware.jpg");
            //Test Ok 19012016
            //ftp.MakeDir("ftp://ftp.tecware.com.br/httpdocs/MakeTecware/testeMak/",@"C:\Users\sebastiao.martins\Documents\WPA Files\TelaTecware 1440x900.jpg");

        }
    }
}

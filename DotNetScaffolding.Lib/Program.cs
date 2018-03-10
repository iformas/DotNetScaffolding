using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.IO;

namespace WizLayerGenerator
{

    class Program
    {

        static void Main(string[] args)
        {

            Program.GeneraDataLayer();
            Console.WriteLine("FIN");
            Console.ReadLine();

        }

        /// <summary>
        /// serviceFactory - servicelayer/ServiceFactory 
        /// RepositoryFactory - dataaccessLayer/RepositoryFactory
        /// </summary>
        public static void GeneraDataLayer()
        {

            /*
             * 0 namespace
             * 1 Clase nombre tabla Capital
             * 2 nombre tabla
             * 3 Contexto 
             */

            string path = ConfigurationManager.AppSettings["pathDatalayer"].ToString();
            string pathService = ConfigurationManager.AppSettings["pathServicelayer"].ToString();
            string pathControllers = ConfigurationManager.AppSettings["pathControllers"].ToString();
            string pathParser = ConfigurationManager.AppSettings["pathParser"].ToString();

            Directory.CreateDirectory(path);
            Directory.CreateDirectory(path + "\\repository");
            Directory.CreateDirectory(pathService);
            Directory.CreateDirectory(pathControllers);
            Directory.CreateDirectory(pathParser);



            string contexto = ConfigurationManager.AppSettings["context"].ToString();
            string strNamespace = ConfigurationManager.AppSettings["ns"].ToString();
            string template = "";
            // List<string> tablas = obtieneTablas();
            List<string> tablas = new List<string>();
            string basedatos = ConfigurationManager.AppSettings["bd"].ToString();
            var oConn = new SqlConnection();
            oConn.ConnectionString = ConfigurationManager.ConnectionStrings["Test"].ToString();

            try
            {
                string sql = "SELECT TABLE_NAME FROM " + basedatos +
                             ".INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'";
                oConn.Open();
                SqlCommand oCmd = new SqlCommand(sql, oConn);
                SqlDataReader oSqlDr = oCmd.ExecuteReader();
                while (oSqlDr.Read())
                {
                    tablas.Add(oSqlDr["TABLE_NAME"].ToString());
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);


            }
            finally
            {
                oConn.Close();
            }

            StringBuilder serviceRepository = new StringBuilder();

            serviceRepository.AppendLine("namespace " + strNamespace + ".servicelayer");
            serviceRepository.AppendLine("{");
            serviceRepository.AppendLine("public class ServiceFactory");
            serviceRepository.AppendLine("{");
            serviceRepository.AppendLine("    private static ServiceFactory _instance = null;");
            serviceRepository.AppendLine();
            serviceRepository.AppendLine("    public static ServiceFactory Instance");
            serviceRepository.AppendLine("    {");
            serviceRepository.AppendLine("        get { return _instance ?? (_instance = new ServiceFactory()); }");
            serviceRepository.AppendLine("    }");
            serviceRepository.AppendLine();


            StringBuilder repositoryFactory = new StringBuilder();


            repositoryFactory.AppendLine("using " + strNamespace + ".dataaccesslayer.repository;");
            repositoryFactory.AppendLine("using " + strNamespace + ".entitylayer.Models;");
            repositoryFactory.AppendLine();
            repositoryFactory.AppendLine("namespace cl.bm.mdlm.sgr.dataaccesslayer");
            repositoryFactory.AppendLine("{");
            repositoryFactory.AppendLine("    public class RepositoryFactory");
            repositoryFactory.AppendLine("    {");
            repositoryFactory.AppendLine("        private static RepositoryFactory _instance = null;");
            repositoryFactory.AppendLine();
            repositoryFactory.AppendLine("        public static RepositoryFactory Instance");
            repositoryFactory.AppendLine("        {");
            repositoryFactory.AppendLine("            get { return _instance ?? (_instance = new RepositoryFactory()); }");
            repositoryFactory.AppendLine("        }");
            repositoryFactory.AppendLine();
            repositoryFactory.AppendLine("        private RepositoryFactory() { } ");
            repositoryFactory.AppendLine();
            repositoryFactory.AppendLine("        public mdlmContext Context()");
            repositoryFactory.AppendLine("        {");
            repositoryFactory.AppendLine("            var context = Usuario().Context;");
            repositoryFactory.AppendLine("            context.Configuration.ProxyCreationEnabled = false;");
            repositoryFactory.AppendLine("            context.Configuration.LazyLoadingEnabled = false;");
            repositoryFactory.AppendLine("            context.Configuration.LazyLoadingEnabled = false;");
            repositoryFactory.AppendLine("            return context;");
            repositoryFactory.AppendLine("        }");
            repositoryFactory.AppendLine();

            foreach (var tabla in tablas)
            {
                List<Campo> campos = new List<Campo>();

                StringBuilder query = new StringBuilder();
                //query.Append("SELECT");
                //query.Append("    c.name 'COLUMN_NAME',");
                //query.Append("    t.Name 'DATA_TYPE',");
                //query.Append("    c.max_length 'MAX_LENGTH',");
                //query.Append("    c.precision ,");
                //query.Append("    c.scale ,");
                //query.Append("    c.is_nullable,");
                //query.Append("    ISNULL(i.is_primary_key, 0) 'PK'");
                //query.Append("    FROM sys.columns c");
                //query.Append("        INNER JOIN ");
                //query.Append("            sys.types t ON c.user_type_id = t.user_type_id");
                //query.Append("        LEFT OUTER JOIN ");
                //query.Append("            sys.index_columns ic ON ic.object_id = c.object_id AND ic.column_id = c.column_id");
                //query.Append("        LEFT OUTER JOIN ");
                //query.Append("            sys.indexes i ON ic.object_id = i.object_id AND ic.index_id = i.index_id");
                //query.Append("    WHERE");
                //query.Append("        c.object_id = OBJECT_ID('" + tabla + "')");

                query.Append("SELECT DISTINCT");
                query.Append("  FirstSet.COLUMN_NAME,");
                query.Append("  FirstSet.DATA_TYPE,");
                query.Append("  FirstSet.MAX_LENGTH,");
                query.Append("  FirstSet. PRECISION,");
                query.Append("  FirstSet.scale,");
                query.Append("  FirstSet.is_nullable,");
                query.Append("  FirstSet.PK,");
                query.Append("  SecondSet.FK_ORIGIN_TABLE_NAME");
                query.Append(" FROM");
                query.Append("  (");
                query.Append("    SELECT");
                query.Append("      c.name 'COLUMN_NAME',");
                query.Append("      t.Name 'DATA_TYPE',");
                query.Append("      c.max_length 'MAX_LENGTH',");
                query.Append("      c. PRECISION,");
                query.Append("      c.scale,");
                query.Append("      c.is_nullable,");
                query.Append("      ISNULL(i.is_primary_key, 0) 'PK'");
                query.Append("    FROM");
                query.Append("      sys.columns c");
                query.Append("    INNER JOIN sys.types t ON c.user_type_id = t.user_type_id");
                query.Append("    LEFT OUTER JOIN sys.index_columns ic ON ic.object_id = c.object_id");
                query.Append("    AND ic.column_id = c.column_id");
                query.Append("    LEFT OUTER JOIN sys.indexes i ON ic.object_id = i.object_id");
                query.Append("    AND ic.index_id = i.index_id");
                query.Append("    WHERE");
                query.Append("      c.object_id = OBJECT_ID('" + tabla + "')");
                query.Append("  ) AS FirstSet");
                query.Append(" LEFT OUTER JOIN (");
                query.Append("  SELECT");
                query.Append("    KCU1.TABLE_NAME AS 'FK_TABLE_NAME',");
                query.Append("    KCU1.COLUMN_NAME AS 'FK_COLUMN_NAME',");
                query.Append("    KCU2.TABLE_NAME AS 'FK_ORIGIN_TABLE_NAME'");
                query.Append("  FROM");
                query.Append("    INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS RC");
                query.Append("  JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE KCU1 ON KCU1.CONSTRAINT_CATALOG = RC.CONSTRAINT_CATALOG");
                query.Append("  AND KCU1.CONSTRAINT_SCHEMA = RC.CONSTRAINT_SCHEMA");
                query.Append("  AND KCU1.CONSTRAINT_NAME = RC.CONSTRAINT_NAME");
                query.Append("  JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE KCU2 ON KCU2.CONSTRAINT_CATALOG = RC.UNIQUE_CONSTRAINT_CATALOG");
                query.Append("  AND KCU2.CONSTRAINT_SCHEMA = RC.UNIQUE_CONSTRAINT_SCHEMA");
                query.Append("  AND KCU2.CONSTRAINT_NAME = RC.UNIQUE_CONSTRAINT_NAME");
                query.Append("  AND KCU2.ORDINAL_POSITION = KCU1.ORDINAL_POSITION");
                query.Append(" ) AS SecondSet ON FirstSet.COLUMN_NAME = SecondSet.FK_COLUMN_NAME  AND SecondSet.FK_TABLE_NAME = '" + tabla + "'");
                var asd = query.ToString();

                try
                {
                    oConn.Open();
                    SqlCommand oCmd = new SqlCommand(query.ToString(), oConn);
                    SqlDataReader oSqlDr = oCmd.ExecuteReader();
                    while (oSqlDr.Read())
                    {
                        campos.Add(new Campo(oSqlDr["COLUMN_NAME"].ToString(), oSqlDr["DATA_TYPE"].ToString(), oSqlDr["PK"].ToString(), oSqlDr["FK_ORIGIN_TABLE_NAME"].ToString() == "" ? null : oSqlDr["FK_ORIGIN_TABLE_NAME"].ToString()));
                    }

                }
                catch (Exception ex) { Console.WriteLine(ex.Message); }
                finally { oConn.Close(); }


                var tab = NornalizeTableName(Char.ToUpper(tabla.First()) + tabla.Substring(1));
                var tablaName = NornalizeTableName(tabla);

                if (tabla.ToUpper() != "Sysdiagrams".ToUpper())
                {
                    BuildRepository(strNamespace, tab, contexto, tablaName, path);
                    BuildServices(strNamespace, tab, tablaName, pathService, campos);
                    BuildParser(strNamespace, tab, tablaName, pathParser, campos);
                    //GeneraControladores(strNamespace, tab, "", tabla, pathControllers, campos);
                }

                repositoryFactory.AppendLine("        public I" + tab + "Repository " + tab + "() ");
                repositoryFactory.AppendLine("        { ");
                repositoryFactory.AppendLine("            return " + tab + "Repository.Instance;");
                repositoryFactory.AppendLine("        }");
                repositoryFactory.AppendLine();

                serviceRepository.AppendLine("        public I" + tab + "Service " + tab + "() ");
                serviceRepository.AppendLine("        {");
                serviceRepository.AppendLine("            return " + tab + "Service.Instance;");
                serviceRepository.AppendLine("        }");
                serviceRepository.AppendLine();

            }

            serviceRepository.AppendLine();
            serviceRepository.AppendLine("    }");
            serviceRepository.AppendLine("}");
            repositoryFactory.AppendLine();
            repositoryFactory.AppendLine();
            repositoryFactory.AppendLine("    }");
            repositoryFactory.AppendLine("}");
            repositoryFactory.AppendLine("");

            File.WriteAllText(path + "\\RepositoryFactory.cs", repositoryFactory.ToString());
            File.WriteAllText(pathService + "\\ServiceFactory.cs", serviceRepository.ToString());
            // WriteAllText creates a file, writes the specified string to the file,
            // and then closes the file.    You do NOT need to call Flush() or Close().
        }

        /// <summary>
        /// dataaccesslayer/repository
        /// </summary>
        /// <param name="strNamespace"></param>
        /// <param name="tab"></param>
        /// <param name="contexto"></param>
        /// <param name="tabla"></param>
        /// <param name="path"></param>
        public static void BuildRepository(string strNamespace, string tab, string contexto, string tabla, string path)
        {
            StringBuilder sourceCode = new StringBuilder();
            sourceCode.AppendLine("using " + strNamespace + ".dataaccesslayer.@base;");
            sourceCode.AppendLine("using " + strNamespace + ".entitylayer.Models;");
            sourceCode.AppendLine("using log4net;");
            sourceCode.AppendLine();
            sourceCode.AppendLine("namespace " + strNamespace + ".dataaccesslayer.repository");
            sourceCode.AppendLine("{");
            sourceCode.AppendLine("    public interface I" + tab + "Repository : IRepository<" + contexto + ", " + tabla + ">");
            sourceCode.AppendLine("    {");
            sourceCode.AppendLine("    }");
            sourceCode.AppendLine();
            sourceCode.AppendLine("    /*°º¤ø,¸¸,ø¤º°`°º¤ø,¸,ø¤°º¤ø,¸¸,ø¤º°`°º¤ø,¸°º¤ø,¸¸,ø¤º°`°º¤ø,¸,ø¤°º¤ø,*/");
            sourceCode.AppendLine("    /*(¯`·._.··¸.-~*´¨¯¨`*·~-.,-(IMPLEMENTACION)-,.-~*´¨¯¨`*·~-.¸··._.·´¯)*/");
            sourceCode.AppendLine("    /*°º¤ø,¸¸,ø¤º°`°º¤ø,¸,ø¤°º¤ø,¸¸,ø¤º°`°º¤ø,¸°º¤ø,¸¸,ø¤º°`°º¤ø,¸,ø¤°º¤ø,*/");
            sourceCode.AppendLine();
            sourceCode.AppendLine("    public class " + tab + "Repository : Repository<" + contexto + ", " + tabla + ">, I" + tab + "Repository   ");
            sourceCode.AppendLine("    {");
            sourceCode.AppendLine("        private static readonly ILog Log = LogManager.GetLogger(typeof(" + tab + "Repository));");
            sourceCode.AppendLine();
            sourceCode.AppendLine("        private " + tab + "Repository()");
            sourceCode.AppendLine("        {");
            sourceCode.AppendLine("            log4net.Config.XmlConfigurator.Configure();");
            sourceCode.AppendLine("        }");
            sourceCode.AppendLine();
            sourceCode.AppendLine("    public static I" + tab + "Repository Instance");
            sourceCode.AppendLine("        {");
            sourceCode.AppendLine("            get { return new " + tab + "Repository(); }");
            sourceCode.AppendLine("        }");
            sourceCode.AppendLine();
            sourceCode.AppendLine("       /**************************** ***** ****************************/");
            sourceCode.AppendLine();
            sourceCode.AppendLine();
            sourceCode.AppendLine();
            sourceCode.AppendLine();
            sourceCode.AppendLine("        /**************************** ***** ****************************/");
            sourceCode.AppendLine("    }");
            sourceCode.AppendLine("}");

            //  sourceCode =String.Format(template,new {strNamespace,tab,tabla,contexto});

            File.WriteAllText(path + "\\repository\\" + tab + "Repository.cs", sourceCode.ToString());
        }

        /// <summary>
        /// servicelayer
        /// </summary>
        /// <param name="strNamespace"></param>
        /// <param name="tab"></param>
        /// <param name="pathService"></param>
        /// <param name="tabla"></param>
        /// <param name="campos"></param>
        public static void BuildServices(string strNamespace, string tab, string tabla, string pathService, List<Campo> campos)
        {
            List<Campo> signatireData = new List<Campo>();
            if (campos.Any(x => x.columna == "usuarioActualizacion")) signatireData.Add(campos.First(x => x.columna == "usuarioActualizacion"));

            String signature = SignatureBuilder(signatireData);
            String signatureFields = String.Join(",", signatireData.Select(x => x.columna));

            var asd = DateTime.Now;
            StringBuilder serviceRep = new StringBuilder();
            serviceRep.AppendLine("using System;");
            serviceRep.AppendLine("using System.Collections.Generic;");
            serviceRep.AppendLine("using System.Linq;");
            serviceRep.AppendLine("using System.Linq.Expressions;");
            serviceRep.AppendLine("using " + strNamespace + ".dataaccesslayer;");
            serviceRep.AppendLine("using " + strNamespace + ".entitylayer.Models;");
            serviceRep.AppendLine("using " + strNamespace + ".servicelayer.exception;");
            serviceRep.AppendLine("using log4net;");
            serviceRep.AppendLine();
            serviceRep.AppendLine("namespace " + strNamespace + ".servicelayer");
            serviceRep.AppendLine("{");
            serviceRep.AppendLine("     public interface I" + tab + "Service");
            serviceRep.AppendLine("     { ");
            serviceRep.AppendLine("         List<" + tabla + "> List" + tab + "(" + (campos.Any(x => x.columna == "eliminado") ? "bool todos = true" : "") + " );");
            serviceRep.AppendLine("         " + tabla + " Get" + tab + "ById(int id);");
            serviceRep.AppendLine("         " + tabla + " Add" + tab + "(" + tabla + " input" + signature + ");");
            serviceRep.AppendLine("         " + tabla + " Edit" + tab + "(" + tabla + " input" + signature + ");");
            serviceRep.AppendLine("         void Delete" + tab + "(int id" + signature + ");");
            serviceRep.AppendLine("     }");
            serviceRep.AppendLine();
            serviceRep.AppendLine("public class " + tab + "Service : I" + tab + "Service ");
            serviceRep.AppendLine("{");
            serviceRep.AppendLine("    private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);");
            serviceRep.AppendLine("");
            serviceRep.AppendLine("    private static " + tab + "Service _instance;");
            serviceRep.AppendLine();
            serviceRep.AppendLine("    public static I" + tab + "Service Instance");
            serviceRep.AppendLine("    {");
            serviceRep.AppendLine("        get { return _instance ?? (_instance = new " + tab + "Service()); }");
            serviceRep.AppendLine("    }");
            serviceRep.AppendLine();
            serviceRep.AppendLine("    private " + tab + "Service()");
            serviceRep.AppendLine("    {");
            serviceRep.AppendLine("        log4net.Config.XmlConfigurator.Configure();");
            serviceRep.AppendLine("    }");
            serviceRep.AppendLine();
            serviceRep.AppendLine("    #region " + tab.ToUpper());
            serviceRep.AppendLine();
            serviceRep.AppendLine("    /**" + tab.ToUpper() + "**/   ");
            serviceRep.AppendLine();
            serviceRep.AppendLine("    public List<" + tabla + "> List" + tab + "(" + (campos.Any(x => x.columna == "eliminado") ? "bool todos = true" : "") + " )");
            serviceRep.AppendLine("    {");
            serviceRep.AppendLine("        using (var repo = RepositoryFactory.Instance." + tab + "()) ");
            serviceRep.AppendLine("        {");
            if (campos.Any(x => x.columna == "eliminado"))
            {
                serviceRep.AppendLine("            if (todos) return repo.FindAll().ToList();");
                serviceRep.AppendLine("            return repo.FindBy(x => !x.eliminado).ToList();");
            }
            else
            {
                serviceRep.AppendLine("            return repo.FindAll().ToList();");
            }
            serviceRep.AppendLine("        }");
            serviceRep.AppendLine("    }");
            serviceRep.AppendLine();
            serviceRep.AppendLine("    public " + tabla + " Get" + tab + "ById(int id) ");
            serviceRep.AppendLine("    {");
            serviceRep.AppendLine("        try");
            serviceRep.AppendLine("        {");
            serviceRep.AppendLine("            using (var r" + tab + " = RepositoryFactory.Instance." + tab + "())");
            serviceRep.AppendLine("            {");
            serviceRep.AppendLine("                return r" + tab + ".FindById(id);");
            serviceRep.AppendLine("            }");
            serviceRep.AppendLine("        }");
            serviceRep.AppendLine("        catch (Exception e)");
            serviceRep.AppendLine("        {");
            serviceRep.AppendLine("             throw new ServiceException(\"NOK\", \"Error de infraestructura al obtener " + tab + "\", e);");
            serviceRep.AppendLine("        }");
            serviceRep.AppendLine("    }");
            serviceRep.AppendLine();
            serviceRep.AppendLine("    public " + tabla + " Add" + tab + "(" + tabla + " input  " + signature + ")");
            serviceRep.AppendLine("    {");
            serviceRep.AppendLine("        try");
            serviceRep.AppendLine("        {");
            serviceRep.AppendLine("            using (var r" + tab + " = RepositoryFactory.Instance." + tab + "())");
            serviceRep.AppendLine("            {");
            if (campos.Any(x => x.columna == "fechaCracion" && GetClrType(x.tipo) == "DateTime"))
            {
                serviceRep.AppendLine("             input.fechaCracion = DateTime.Now;");
            }
            if (campos.Any(x => x.columna == "fechaActualizacion" && GetClrType(x.tipo) == "DateTime"))
            {
                serviceRep.AppendLine("             input.fechaActualizacion = DateTime.Now;");
            }
            if (campos.Any(x => x.columna == "usuarioActualizacion"))
            {
                serviceRep.AppendLine("             input.usuarioActualizacion = usuarioActualizacion;");
            }
            serviceRep.AppendLine("            return r" + tab + ".Add(input);");
            serviceRep.AppendLine("            }");
            serviceRep.AppendLine("        }");
            serviceRep.AppendLine("        catch (Exception e)");
            serviceRep.AppendLine("        {");
            serviceRep.AppendLine("             throw new ServiceException(\"NOK\", \"Error de infraestructura al agregar " + tab + "\", e);");
            serviceRep.AppendLine("        }");
            serviceRep.AppendLine("    }");
            serviceRep.AppendLine();
            serviceRep.AppendLine("    public " + tabla + " Edit" + tab + "(" + tabla + " input" + signature + ")");
            serviceRep.AppendLine("    {");
            serviceRep.AppendLine("        try");
            serviceRep.AppendLine("        {");
            serviceRep.AppendLine("            using (var r" + tab + " = RepositoryFactory.Instance." + tab + "())");
            serviceRep.AppendLine("            {");
            if (campos.Any(x => x.columna == "fechaActualizacion" && GetClrType(x.tipo) == "DateTime"))
            {
                serviceRep.AppendLine("             input.fechaActualizacion = DateTime.Now;");
            }
            if (campos.Any(x => x.columna == "usuarioActualizacion"))
            {
                serviceRep.AppendLine("             input.usuarioActualizacion = usuarioActualizacion;");
            }
            serviceRep.AppendLine("                return r" + tab + ".Set(input);");
            serviceRep.AppendLine("            }");
            serviceRep.AppendLine("        }");
            serviceRep.AppendLine("        catch (Exception e)");
            serviceRep.AppendLine("        {");
            serviceRep.AppendLine("             throw new ServiceException(\"NOK\", \"Error de infraestructura al agregar " + tab + "\", e);");
            serviceRep.AppendLine("        }");
            serviceRep.AppendLine("    }");
            serviceRep.AppendLine();
            serviceRep.AppendLine("    public void Delete" + tab + " (int id" + signature + ")");
            serviceRep.AppendLine("    {");
            serviceRep.AppendLine("        try");
            serviceRep.AppendLine("        {");
            serviceRep.AppendLine("            using (var repo = RepositoryFactory.Instance." + tab + "())");
            serviceRep.AppendLine("            {");
            serviceRep.AppendLine("                var data = repo.FindById(id);");
            if (campos.Any(x => x.columna == "eliminado" && GetClrType(x.tipo) == "bool"))
            {
                serviceRep.AppendLine();
                serviceRep.AppendLine("                if (data != null)");
                serviceRep.AppendLine("                {");
                serviceRep.AppendLine("                    data.eliminado = true;");
                serviceRep.AppendLine("                    Edit" + tab + " (data" + (signatureFields.Trim() != "" ? ("," + signatureFields) : String.Empty) + ");");
                serviceRep.AppendLine("                }");
            }
            else
            {
                serviceRep.AppendLine("                repo.Delete(data);");
            }
            serviceRep.AppendLine("            }");
            serviceRep.AppendLine("        }");
            serviceRep.AppendLine("        catch (Exception e)");
            serviceRep.AppendLine("        {");
            serviceRep.AppendLine("            throw new ServiceException(\"NOK\", \"Error de infraestructura al eliminar " + tab + "\", e);");
            serviceRep.AppendLine("        }");
            serviceRep.AppendLine("    }");
            serviceRep.AppendLine();
            serviceRep.AppendLine("    /**FIN " + tab.ToUpper() + "**/ ");
            serviceRep.AppendLine("    #endregion");
            serviceRep.AppendLine("}");
            serviceRep.AppendLine("}");

            File.WriteAllText(pathService + "\\" + tab + "Service.cs", serviceRep.ToString());
        }



        private static void BuildParser(string strNamespace, string tab, string tabla, string pathParser, List<Campo> campos)
        {
            StringBuilder parser =  new StringBuilder();
            List<String> fkTablesWritted = new List<string>();
            parser.AppendLine("using System.Collections.Generic;");
            parser.AppendLine("using System.Linq;");
            parser.AppendLine("using cl.bm.mdlm.sgr.entitylayer.Models;");
            parser.AppendLine();
            parser.AppendLine("namespace " + strNamespace + ".Models.Parser");
            parser.AppendLine("{");
            parser.AppendLine();
            parser.AppendLine("    public static class " + tab + "Json");
            parser.AppendLine("    {");
            parser.AppendLine("        public static " + tabla + " ToJson(this " + tabla + " data)");
            parser.AppendLine("        {");
            parser.AppendLine("            return Parser(data);");
            parser.AppendLine("        }");
            parser.AppendLine();
            parser.AppendLine("        public static List<" + tabla + "> ToJson(this List<" + tabla + "> data)");
            parser.AppendLine("        {");
            parser.AppendLine("            return data.Select(Parser).ToList();");
            parser.AppendLine("        }");
            parser.AppendLine();
            parser.AppendLine("        private static " + tabla + " Parser(" + tabla + " s)");
            parser.AppendLine("        {");
            parser.AppendLine("            return new " + tabla);
            parser.AppendLine("            {");
            foreach (var campo in campos)
            {
                parser.AppendLine("                " + campo.columna + " = s." + campo.columna + ",");
                if (campo.fkOriginTable != null)
                {
                    String fkOriginTable = NornalizeTableName(campo.fkOriginTable);
                    String propertyname = fkOriginTable;
                    int countWritted = fkTablesWritted.Count(x => x == fkOriginTable);
                    if (countWritted > 0)
                    {
                        propertyname = fkOriginTable + countWritted;
                        fkTablesWritted.Add(fkOriginTable);
                    }
                    parser.AppendLine("                " + propertyname + " = s." + fkOriginTable + "  == null ? null : s." + fkOriginTable + ".ToJson(),");
                }
            }

            parser.AppendLine("            };");
            parser.AppendLine("        }");
            parser.AppendLine("    }");
            parser.AppendLine("}");
            parser.AppendLine();
            File.WriteAllText(pathParser + "\\" + tab + "Parser.cs", parser.ToString());
        }

        private static void GeneraControladores(string strNamespace, string tab, string contexto, string tabla, string path, List<Campo> campos)
        {
            throw new NotImplementedException();
        }

        public static string GetClrType(string sqlType)
        {
            switch (sqlType)
            {
                case "bigint":
                    return "long";

                case "binary":
                case "image":
                case "timestamp":
                case "varbinary":
                    return "byte[]";

                case "bit":
                    return "bool";

                case "char":
                case "nchar":
                case "ntext":
                case "nvarchar":
                case "text":
                case "varchar":
                case "xml":
                    return "string";

                case "datetime":
                case "smalldatetime":
                case "date":
                case "time":
                case "dateTime2":
                case "datetime2":
                    return "DateTime";

                case "decimal":
                case "money":
                case "smallmoney":
                    return "decimal";

                case "float":
                    return "double";

                case "int":
                    return "int";

                case "real":
                    return "float";

                case "uniqueidentifier":
                    return "Guid";

                case "smallint":
                    return "short";

                case "tinyint":
                    return typeof(byte?).ToString();

                case "variant":
                case "udt":
                    return "object";



                default:
                    throw new ArgumentOutOfRangeException("sqlType");
            }
        }

        private static String SignatureBuilder(List<Campo> campos)
        {
            String result = String.Empty;
            foreach (var campo in campos)
            {
                result += ", " + GetClrType(campo.tipo) + " " + campo.columna;
            }
            return result;
        }

        private static String NornalizeTableName(String tab)
        {
            return tab.Substring(tab.Length - 1).ToLower() == "s" ? tab.Remove(tab.Length - 1) : tab;
        }


        private static List<string> obtieneTablas()
        {
            List<string> tablas = new List<string>();
            string basedatos = ConfigurationManager.AppSettings["bd"].ToString();
            SqlConnection oConn = new SqlConnection(ConfigurationManager.AppSettings["Test"].ToString());
            try
            {
                string sql = "SELECT TABLE_NAME FROM " + basedatos +
                             ".INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'";
                oConn.Open();
                SqlCommand oCmd = new SqlCommand(sql, oConn);
                SqlDataReader oSqlDr = oCmd.ExecuteReader();
                while (oSqlDr.Read())
                {
                    tablas.Add(oSqlDr["TABLE_NAME"].ToString());
                }
                return tablas;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return tablas;

            }
            finally
            {
                oConn.Close();
            }
        }
    }

}
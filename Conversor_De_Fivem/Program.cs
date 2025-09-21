using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace VehiclesMetaMerger
{
    class Program
    {
        // Mapeamento de nomes de arquivos para os nós que devem ser mesclados dentro deles.
        private static readonly Dictionary<string, (string RootName, string[] ChildNodes)> MetaFileConfig = new Dictionary<string, (string, string[])>
        {
            ["vehicles"] = ("CVehicleModelInfo__InitDataList", new[] { "InitDatas", "txdRelationships" }),
            ["carcols"] = ("CVehicleModelInfoVarGlobal", new[] { "Kits", "Lights", "Sirens" }),
            ["carvariations"] = ("CVehicleModelInfoVariation", new[] { "variationData" }),
            ["handling"] = ("CHandlingDataMgr", new[] { "HandlingData" }),
            ["vehiclelayouts"] = ("CVehicleMetadataMgr", new[] {
                "AnimRateSets", "ClipSetMaps", "VehicleCoverBoundOffsetInfos", "BicycleInfos", "POVTuningInfos",
                "EntryAnimVariations", "VehicleExtraPointsInfos", "DrivebyWeaponGroups", "VehicleDriveByAnimInfos",
                "VehicleDriveByInfos", "VehicleSeatInfos", "VehicleSeatAnimInfos", "VehicleEntryPointInfos",
                "VehicleEntryPointAnimInfos", "VehicleExplosionInfos", "VehicleLayoutInfos", "VehicleScenarioLayoutInfos",
                "SeatOverrideAnimInfos", "InVehicleOverrideInfos", "FirstPersonDriveByLookAroundData"
            })
        };

        static void Main(string[] args)
        {
            Console.Title = "Conversor de Fivem para GTA";
            WriteLineColor("LSPD:BR - Créditos: mmVehiclesMetaMerger por mmleczek.com para reective.com (convertido para C#)", ConsoleColor.Cyan);

            ProgramStart();
            AskForActionLoop();
        }

        /// <summary>
        /// Garante que todos os diretórios necessários existam.
        /// </summary>
        private static void ProgramStart()
        {
            try
            {
                foreach (var key in MetaFileConfig.Keys)
                {
                    Directory.CreateDirectory($"{key}_meta");
                }
                Directory.CreateDirectory("output");
                File.WriteAllText("errors.txt", string.Empty); // Limpa o log de erros na inicialização
            }
            catch (Exception ex)
            {
                WriteLineColor($"Erro ao inicializar diretórios: {ex.Message}", ConsoleColor.Red);
            }
        }

        /// <summary>
        /// Exibe o menu de opções para o usuário.
        /// </summary>
        private static void DisplayMenu()
        {
            Console.WriteLine("\n+-----------------------------------------------------------------+");
            Console.WriteLine("| Escolha uma ação:                                               |");
            Console.WriteLine("+-----------------------------------------------------------------+");
            Console.WriteLine("| 1.  Mesclar vehicles.meta                                       |");
            Console.WriteLine("| 2.  Mesclar carcols.meta                                        |");
            Console.WriteLine("| 3.  Mesclar carvariations.meta                                  |");
            Console.WriteLine("| 4.  Mesclar handling.meta                                       |");
            Console.WriteLine("| 5.  Mesclar vehiclelayouts.meta                                 |");
            Write("| 6.  "); WriteColor("Mesclar todos os anteriores", ConsoleColor.Cyan); Console.WriteLine("                                   |");
            Write("| 7.  "); WriteColor("Importar todos vehicles.meta de um diretório", ConsoleColor.Magenta); Console.WriteLine("          |");
            Write("| 8.  "); WriteColor("Importar todos carcols.meta de um diretório", ConsoleColor.Magenta); Console.WriteLine("           |");
            Write("| 9.  "); WriteColor("Importar todos carvariations.meta de um diretório", ConsoleColor.Magenta); Console.WriteLine("     |");
            Write("| 10. "); WriteColor("Importar todos handling.meta de um diretório", ConsoleColor.Magenta); Console.WriteLine("          |");
            Write("| 11. "); WriteColor("Importar todos vehiclelayouts.meta de um diretório", ConsoleColor.Magenta); Console.WriteLine("    |");
            Write("| 12. "); WriteColor("Importar todos os anteriores de um diretório", ConsoleColor.Magenta); Console.WriteLine("        |");
            Write("| 13. "); WriteColor("Importar arquivos por pesquisa de um diretório", ConsoleColor.Magenta); Console.WriteLine("      |");
            Write("| 14. "); WriteColor("Extrair nomes de modelos de vehicles.meta", ConsoleColor.Green); Console.WriteLine("           |");
            Write("| 15. "); WriteColor("Sair", ConsoleColor.Red); Console.WriteLine("                                                     |");
            Console.WriteLine("+-----------------------------------------------------------------+\n");
        }

        /// <summary>
        /// Loop principal que exibe o menu e processa a entrada do usuário.
        /// </summary>
        private static void AskForActionLoop()
        {
            while (true)
            {
                DisplayMenu();
                Console.Write("Escolha uma ação (1 - 15): ");
                string input = Console.ReadLine();

                if (int.TryParse(input, out int choice) && choice >= 1 && choice <= 15)
                {
                    switch (choice)
                    {
                        case 1: PerformMerge("vehicles"); break;
                        case 2: PerformMerge("carcols"); break;
                        case 3: PerformMerge("carvariations"); break;
                        case 4: PerformMerge("handling"); break;
                        case 5: PerformMerge("vehiclelayouts"); break;
                        case 6:
                            PerformMerge("vehicles");
                            PerformMerge("carcols");
                            PerformMerge("carvariations");
                            PerformMerge("handling");
                            PerformMerge("vehiclelayouts");
                            break;
                        case 7: ImportFiles("vehicles"); break;
                        case 8: ImportFiles("carcols"); break;
                        case 9: ImportFiles("carvariations"); break;
                        case 10: ImportFiles("handling"); break;
                        case 11: ImportFiles("vehiclelayouts"); break;
                        case 12:
                            ImportFiles("vehicles");
                            ImportFiles("carcols");
                            ImportFiles("carvariations");
                            ImportFiles("handling");
                            ImportFiles("vehiclelayouts");
                            break;
                        case 13: ImportByQuery(); break;
                        case 14: ExtractModelNames(); break;
                        case 15: Environment.Exit(0); break;
                    }
                }
                else
                {
                    WriteLineColor("Entrada inválida. Por favor, escolha um número entre 1 e 15.", ConsoleColor.Red);
                }
            }
        }

        /// <summary>
        /// Invoca a função genérica de mesclagem para um tipo de meta específico.
        /// </summary>
        private static void PerformMerge(string metaKey)
        {
            WriteLineColor($"Mesclando todos os arquivos {metaKey}.meta...", ConsoleColor.Cyan);
            var config = MetaFileConfig[metaKey];
            MergeMetaFiles($"{metaKey}_meta", $"output/{metaKey}.meta", config.RootName, config.ChildNodes);
        }

        /// <summary>
        /// Invoca a função genérica de importação para um tipo de meta específico.
        /// </summary>
        private static void ImportFiles(string metaKey)
        {
            WriteLineColor($"Importando todos os arquivos {metaKey}.meta...", ConsoleColor.Magenta);
            Console.Write("Caminho para o diretório: ");
            string sourceDir = Console.ReadLine();
            ImportFromDirectory($"{metaKey}_meta", sourceDir, $"**/{metaKey}.meta");
        }

        /// <summary>
        /// Lógica genérica para mesclar arquivos XML.
        /// </summary>
        private static void MergeMetaFiles(string sourceFolder, string outputFile, string rootNodeName, string[] nodesToMerge)
        {
            try
            {
                var files = Directory.GetFiles(sourceFolder, "*.meta");
                if (files.Length == 0)
                {
                    WriteLineColor($"Nenhum arquivo encontrado em '{sourceFolder}'.", ConsoleColor.Yellow);
                    return;
                }

                XDocument baseDoc = XDocument.Load(files[0]);
                XElement root = baseDoc.Element(rootNodeName);

                if (root == null)
                {
                    WriteLineColor($"Erro: O nó raiz '{rootNodeName}' não foi encontrado no arquivo base {files[0]}.", ConsoleColor.Red);
                    return;
                }

                for (int i = 1; i < files.Length; i++)
                {
                    try
                    {
                        XDocument currentDoc = XDocument.Load(files[i]);
                        foreach (var nodeName in nodesToMerge)
                        {
                            var baseNodeContainer = root.Element(nodeName);
                            if (baseNodeContainer == null)
                            {
                                // Se o contêiner não existe no arquivo base, cria-o.
                                baseNodeContainer = new XElement(nodeName);
                                root.Add(baseNodeContainer);
                            }

                            // Adiciona todos os elementos filhos do nó correspondente no arquivo atual.
                            var itemsToAdd = currentDoc.Descendants(nodeName).Elements();
                            baseNodeContainer.Add(itemsToAdd);
                        }
                    }
                    catch (Exception ex)
                    {
                        string errorMsg = $"Erro ao processar o arquivo {files[i]}: {ex.Message}";
                        WriteLineColor(errorMsg, ConsoleColor.Red);
                        File.AppendAllText("errors.txt", errorMsg + Environment.NewLine);
                    }
                }

                // Remove nós vazios que podem ter sido criados
                foreach (var nodeName in nodesToMerge)
                {
                    var container = root.Element(nodeName);
                    if (container != null && !container.HasElements)
                    {
                        container.Remove();
                    }
                }

                baseDoc.Save(outputFile);
                WriteLineColor($"Mesclagem de '{outputFile}' concluída com sucesso!", ConsoleColor.Green);
            }
            catch (Exception ex)
            {
                string errorMsg = $"Um erro fatal ocorreu durante a mesclagem para {outputFile}: {ex.Message}";
                WriteLineColor(errorMsg, ConsoleColor.Red);
                File.AppendAllText("errors.txt", errorMsg + Environment.NewLine);
            }
        }

        /// <summary>
        /// Lógica genérica para importar arquivos de um diretório de origem.
        /// </summary>
        private static void ImportFromDirectory(string destFolder, string sourceDir, string searchPattern)
        {
            if (!Directory.Exists(sourceDir))
            {
                WriteLineColor("O diretório de origem não existe!", ConsoleColor.Red);
                return;
            }
            try
            {
                var files = Directory.GetFiles(sourceDir, Path.GetFileName(searchPattern), SearchOption.AllDirectories);
                int count = 0;
                foreach (var file in files)
                {
                    string destFileName = Path.Combine(destFolder, $"{Path.GetFileNameWithoutExtension(destFolder)}{count++}.meta");
                    File.Copy(file, destFileName, true);
                }
                WriteLineColor($"{count} arquivo(s) importado(s) para '{destFolder}' com sucesso.", ConsoleColor.Magenta);
            }
            catch (Exception ex)
            {
                WriteLineColor($"Erro durante a importação: {ex.Message}", ConsoleColor.Red);
            }
        }

        /// <summary>
        /// Importa arquivos com base em uma consulta de pesquisa personalizada.
        /// </summary>
        private static void ImportByQuery()
        {
            Console.Write("Caminho para o diretório de origem: ");
            string sourceDir = Console.ReadLine();
            if (!Directory.Exists(sourceDir))
            {
                WriteLineColor("O diretório de origem não existe!", ConsoleColor.Red);
                return;
            }

            Console.Write("Caminho para o diretório de destino: ");
            string destDir = Console.ReadLine();
            if (!Directory.Exists(destDir))
            {
                WriteLineColor("O diretório de destino não existe! Crie-o primeiro.", ConsoleColor.Red);
                return;
            }

            Console.Write("Consulta de pesquisa (ex: *.meta ou handling*): ");
            string query = Console.ReadLine();

            try
            {
                var files = Directory.GetFiles(sourceDir, query, SearchOption.AllDirectories);
                int count = 0;
                foreach (var file in files)
                {
                    string destFileName = Path.Combine(destDir, Path.GetFileName(file));
                    File.Copy(file, destFileName, true);
                    count++;
                }
                WriteLineColor($"{count} arquivo(s) correspondente(s) a '{query}' importado(s) para '{destDir}' com sucesso.", ConsoleColor.Magenta);
            }
            catch (Exception ex)
            {
                WriteLineColor($"Erro durante a importação: {ex.Message}", ConsoleColor.Red);
            }
        }

        /// <summary>
        /// Extrai todos os nós <modelName> de todos os arquivos em vehicles_meta.
        /// </summary>
        private static void ExtractModelNames()
        {
            WriteLineColor("Extraindo nomes de modelos de vehicles.meta...", ConsoleColor.Cyan);
            const string sourceFolder = "vehicles_meta";
            try
            {
                var files = Directory.GetFiles(sourceFolder, "*.meta");
                if (files.Length == 0)
                {
                    WriteLineColor($"Nenhum arquivo encontrado em '{sourceFolder}'.", ConsoleColor.Yellow);
                    return;
                }

                var modelNames = new List<string>();
                foreach (var file in files)
                {
                    try
                    {
                        XDocument doc = XDocument.Load(file);
                        var names = doc.Descendants("modelName").Select(el => el.Value);
                        modelNames.AddRange(names);
                    }
                    catch (Exception ex)
                    {
                        string errorMsg = $"Erro ao processar o arquivo {file}: {ex.Message}";
                        WriteLineColor(errorMsg, ConsoleColor.Red);
                        File.AppendAllText("errors.txt", errorMsg + Environment.NewLine);
                    }
                }

                string outputPath = "output/exportedModelNames.txt";
                File.WriteAllLines(outputPath, modelNames.Distinct()); // Usa Distinct para evitar nomes duplicados
                WriteLineColor($"Extração de nomes de modelos concluída! {modelNames.Count} nomes encontrados. Salvo em '{outputPath}'", ConsoleColor.Green);
            }
            catch (Exception ex)
            {
                WriteLineColor($"Ocorreu um erro durante a extração: {ex.Message}", ConsoleColor.Red);
            }
        }

        #region Funções Auxiliares de Console
        private static void WriteLineColor(string text, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ResetColor();
        }

        private static void WriteColor(string text, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.Write(text);
            Console.ResetColor();
        }

        private static void Write(string text)
        {
            Console.Write(text);
        }
        #endregion
    }
}

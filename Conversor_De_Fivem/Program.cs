using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using HtmlAgilityPack; // Adicionado para correção de XML

namespace mmVehiclesMetaMerger
{
    class Program
    {
        // Estrutura para armazenar os nomes dos elementos a serem mesclados para cada tipo de arquivo meta.
        private static readonly Dictionary<string, string[]> MergeTargets = new Dictionary<string, string[]>
        {
            ["vehicles"] = new[] { "InitDatas", "txdRelationships" },
            ["carcols"] = new[] { "Kits", "Lights", "Sirens" },
            ["carvariations"] = new[] { "variationData" },
            ["handling"] = new[] { "HandlingData" },
            ["vehiclelayouts"] = new[] {
                "AnimRateSets", "ClipSetMaps", "VehicleCoverBoundOffsetInfos", "BicycleInfos", "POVTuningInfos",
                "EntryAnimVariations", "VehicleExtraPointsInfos", "DrivebyWeaponGroups", "VehicleDriveByAnimInfos",
                "VehicleDriveByInfos", "VehicleSeatInfos", "VehicleSeatAnimInfos", "VehicleEntryPointInfos",
                "VehicleEntryPointAnimInfos", "VehicleExplosionInfos", "VehicleLayoutInfos", "VehicleScenarioLayoutInfos",
                "SeatOverrideAnimInfos", "InVehicleOverrideInfos", "FirstPersonDriveByLookAroundData"
            }
        };

        static void Main(string[] args)
        {
            Console.Title = "mmVehiclesMetaMerger by mmleczek.com | Convertido e Adaptado por LSPD:BR";
            PrintColor("mmVehiclesMetaMerger by ", ConsoleColor.White, false);
            PrintColor("mmleczek.com", ConsoleColor.Cyan, false);
            PrintColor(" | Convertido e Adaptado para C# e PT-BR por ", ConsoleColor.White, false);
            PrintColor("LSPD:BR", ConsoleColor.Cyan);

            ProgramStart();
            MainLoop();
        }

        /// <summary>
        /// Loop principal do programa que exibe o menu e processa a entrada do usuário.
        /// </summary>
        private static void MainLoop()
        {
            while (true)
            {
                Console.Clear();
                ShowMenu();
                Console.Write("Escolha uma ação (1 - 15): ");
                string input = Console.ReadLine();

                if (int.TryParse(input, out int choice) && choice >= 1 && choice <= 15)
                {
                    HandleChoice(choice);

                    // Pausa para o usuário ver o resultado antes de limpar, exceto ao sair.
                    if (choice != 15)
                    {
                        Console.WriteLine("\n\nPressione qualquer tecla para voltar ao menu...");
                        Console.ReadKey(true);
                    }
                }
                else
                {
                    PrintColor("Número inválido. O programa aceita apenas números no intervalo de 1 a 15.", ConsoleColor.Red);
                    // Pausa para o usuário ver o erro antes de limpar.
                    System.Threading.Thread.Sleep(2000);
                }
            }
        }

        /// <summary>
        /// Garante que todos os diretórios necessários existam.
        /// </summary>
        private static void ProgramStart()
        {
            try
            {
                Directory.CreateDirectory(Path.Combine(GetDir(), "vehicles_meta"));
                Directory.CreateDirectory(Path.Combine(GetDir(), "carcols_meta"));
                Directory.CreateDirectory(Path.Combine(GetDir(), "carvariations_meta"));
                Directory.CreateDirectory(Path.Combine(GetDir(), "handling_meta"));
                Directory.CreateDirectory(Path.Combine(GetDir(), "vehiclelayouts_meta"));
                Directory.CreateDirectory(Path.Combine(GetDir(), "output"));
            }
            catch (Exception ex)
            {
                PrintColor($"Erro ao criar diretórios iniciais: {ex.Message}", ConsoleColor.Red);
            }
        }

        /// <summary>
        /// Exibe o menu de opções formatado no console.
        /// </summary>
        private static void ShowMenu()
        {
            Console.WriteLine("\n+----+---------------------------------------------------------+");
            Console.WriteLine("| ID | Ação                                                    |");
            Console.WriteLine("+----+---------------------------------------------------------+");
            Console.WriteLine("| 1  | Mesclar vehicles.meta                                   |");
            Console.WriteLine("| 2  | Mesclar carcols.meta                                    |");
            Console.WriteLine("| 3  | Mesclar carvariations.meta                              |");
            Console.WriteLine("| 4  | Mesclar handling.meta                                   |");
            Console.WriteLine("| 5  | Mesclar vehiclelayouts.meta                             |");
            Console.Write("| 6  | "); PrintColor("Mesclar todos os anteriores", ConsoleColor.Cyan, false); Console.WriteLine("                         |");
            Console.Write("| 7  | "); PrintColor("Importar todos os vehicles.meta de um diretório", ConsoleColor.Magenta, false); Console.WriteLine("     |");
            Console.Write("| 8  | "); PrintColor("Importar todos os carcols.meta de um diretório", ConsoleColor.Magenta, false); Console.WriteLine("      |");
            Console.Write("| 9  | "); PrintColor("Importar todos os carvariations.meta de um diretório", ConsoleColor.Magenta, false); Console.WriteLine(" |");
            Console.Write("| 10 | "); PrintColor("Importar todos os handling.meta de um diretório", ConsoleColor.Magenta, false); Console.WriteLine("     |");
            Console.Write("| 11 | "); PrintColor("Importar todos os vehiclelayouts.meta de um diretório", ConsoleColor.Magenta, false); Console.WriteLine(" |");
            Console.Write("| 12 | "); PrintColor("Importar todos os anteriores de um diretório", ConsoleColor.Magenta, false); Console.WriteLine("        |");
            Console.Write("| 13 | "); PrintColor("Importar outros arquivos de um diretório por pesquisa", ConsoleColor.Magenta, false); Console.WriteLine(" |");
            Console.Write("| 14 | "); PrintColor("Extrair nomes de modelos dos arquivos vehicles.meta", ConsoleColor.Green, false); Console.WriteLine(" |");
            Console.Write("| 15 | "); PrintColor("Sair", ConsoleColor.Red, false); Console.WriteLine("                                                 |");
            Console.WriteLine("+----+---------------------------------------------------------+\n");
        }

        /// <summary>
        /// Direciona a escolha do usuário para a função correspondente.
        /// </summary>
        private static void HandleChoice(int choice)
        {
            string dirPath;
            switch (choice)
            {
                case 1:
                    MergeMetaFiles("vehicles");
                    break;
                case 2:
                    MergeMetaFiles("carcols");
                    break;
                case 3:
                    MergeMetaFiles("carvariations");
                    break;
                case 4:
                    MergeMetaFiles("handling");
                    break;
                case 5:
                    MergeMetaFiles("vehiclelayouts");
                    break;
                case 6:
                    MergeMetaFiles("vehicles");
                    MergeMetaFiles("carcols");
                    MergeMetaFiles("carvariations");
                    MergeMetaFiles("handling");
                    MergeMetaFiles("vehiclelayouts");
                    break;
                case 7:
                    dirPath = AskForPath("Caminho para o diretório");
                    if (dirPath != null) ImportFilesFromDir("vehicles", dirPath);
                    break;
                case 8:
                    dirPath = AskForPath("Caminho para o diretório");
                    if (dirPath != null) ImportFilesFromDir("carcols", dirPath);
                    break;
                case 9:
                    dirPath = AskForPath("Caminho para o diretório");
                    if (dirPath != null) ImportFilesFromDir("carvariations", dirPath);
                    break;
                case 10:
                    dirPath = AskForPath("Caminho para o diretório");
                    if (dirPath != null) ImportFilesFromDir("handling", dirPath);
                    break;
                case 11:
                    dirPath = AskForPath("Caminho para o diretório");
                    if (dirPath != null) ImportFilesFromDir("vehiclelayouts", dirPath);
                    break;
                case 12:
                    dirPath = AskForPath("Caminho para o diretório");
                    if (dirPath != null)
                    {
                        ImportFilesFromDir("vehicles", dirPath);
                        ImportFilesFromDir("carcols", dirPath);
                        ImportFilesFromDir("carvariations", dirPath);
                        ImportFilesFromDir("handling", dirPath);
                        ImportFilesFromDir("vehiclelayouts", dirPath);
                    }
                    break;
                case 13:
                    ImportFileByQueryFromDir();
                    break;
                case 14:
                    ExtractModelNamesFromVehiclesMeta();
                    break;
                case 15:
                    Environment.Exit(0);
                    break;
            }
        }

        /// <summary>
        /// Processo genérico para mesclar arquivos .meta.
        /// </summary>
        private static void MergeMetaFiles(string metaType)
        {
            PrintColor($"Mesclando todos os arquivos {metaType}.meta...", ConsoleColor.Cyan);

            string sourceDir = Path.Combine(GetDir(), $"{metaType}_meta");
            string outputFile = Path.Combine(GetDir(), "output", $"{metaType}.meta");
            string[] filesToMerge;

            try
            {
                filesToMerge = Directory.GetFiles(sourceDir, "*.meta", SearchOption.TopDirectoryOnly);
            }
            catch (Exception ex)
            {
                PrintColor($"Erro ao ler o diretório {sourceDir}: {ex.Message}", ConsoleColor.Red);
                return;
            }


            if (filesToMerge.Length == 0)
            {
                PrintColor($"Não foram encontrados arquivos no caminho fornecido: {sourceDir}", ConsoleColor.Yellow);
                return;
            }

            File.WriteAllText(Path.Combine(GetDir(), "errors.txt"), string.Empty);

            XDocument baseDoc = TryFixAndParseXml(filesToMerge[0]);
            if (baseDoc == null || baseDoc.Root == null)
            {
                PrintColor($"O arquivo base '{Path.GetFileName(filesToMerge[0])}' não pôde ser analisado ou está vazio.", ConsoleColor.Red);
                return;
            }
            XElement root = baseDoc.Root;


            // Loop através dos outros arquivos para mesclar
            for (int i = 1; i < filesToMerge.Length; i++)
            {
                XDocument currentDoc = TryFixAndParseXml(filesToMerge[i]);
                if (currentDoc == null || currentDoc.Root == null)
                {
                    // O erro já foi logado dentro de TryFixAndParseXml
                    continue;
                }

                // Itera sobre os nomes dos elementos alvo para o tipo de meta atual.
                foreach (var targetNodeName in MergeTargets[metaType])
                {
                    var targetNode = root.Element(targetNodeName);
                    var sourceNodes = currentDoc.Root.Element(targetNodeName)?.Elements("Item");

                    if (sourceNodes != null && sourceNodes.Any())
                    {
                        // Se o nó de destino não existir no documento base, cria-o.
                        if (targetNode == null)
                        {
                            targetNode = new XElement(targetNodeName);
                            root.Add(targetNode);
                        }
                        targetNode.Add(sourceNodes);
                    }
                }
            }

            // Garante que os nós de destino existam, mesmo que vazios.
            foreach (var targetNodeName in MergeTargets[metaType])
            {
                if (root.Element(targetNodeName) == null)
                {
                    root.Add(new XElement(targetNodeName));
                }
            }

            // Salva o XML mesclado com formatação e sem a declaração XML.
            var settings = new System.Xml.XmlWriterSettings
            {
                OmitXmlDeclaration = true,
                Indent = true,
                Encoding = new UTF8Encoding(false) // UTF-8 without BOM
            };

            try
            {
                using (var writer = System.Xml.XmlWriter.Create(outputFile, settings))
                {
                    baseDoc.Save(writer);
                }
                PrintColor($"Mesclagem de todos os arquivos {metaType}.meta concluída!", ConsoleColor.Green);
            }
            catch (Exception ex)
            {
                PrintColor($"Erro ao salvar o arquivo final {outputFile}: {ex.Message}", ConsoleColor.Red);
            }
        }

        /// <summary>
        /// Extrai todos os <modelName> dos arquivos em vehicles_meta.
        /// </summary>
        private static void ExtractModelNamesFromVehiclesMeta()
        {
            PrintColor("Extraindo nomes de modelo dos arquivos vehicles.meta...", ConsoleColor.Cyan);

            string sourceDir = Path.Combine(GetDir(), "vehicles_meta");
            string outputFile = Path.Combine(GetDir(), "output", "exportedModelNames.txt");

            if (!Directory.Exists(sourceDir))
            {
                PrintColor($"Diretório de origem não encontrado: {sourceDir}", ConsoleColor.Red);
                return;
            }

            var files = Directory.GetFiles(sourceDir, "*.meta");
            if (files.Length == 0)
            {
                PrintColor($"Nenhum arquivo encontrado em {sourceDir}", ConsoleColor.Yellow);
                return;
            }

            var modelNames = new List<string>();
            foreach (var file in files)
            {
                XDocument doc = TryFixAndParseXml(file);
                if (doc != null)
                {
                    var names = doc.Descendants("Item")
                                   .Elements("modelName")
                                   .Select(el => el.Value);
                    modelNames.AddRange(names);
                }
            }

            try
            {
                if (modelNames.Any())
                {
                    File.WriteAllLines(outputFile, modelNames.Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(n => n));
                    PrintColor("Extração de nomes de modelo dos arquivos vehicles.meta concluída!", ConsoleColor.Green);
                }
                else
                {
                    PrintColor("Nenhum nome de modelo foi encontrado nos arquivos.", ConsoleColor.Yellow);
                }
            }
            catch (Exception ex)
            {
                PrintColor($"Erro ao escrever no arquivo de saída {outputFile}: {ex.Message}", ConsoleColor.Red);
            }
        }

        /// <summary>
        /// Importa arquivos .meta de um diretório de origem para o diretório de trabalho.
        /// </summary>
        private static void ImportFilesFromDir(string metaType, string sourceDirectory)
        {
            PrintColor($"Importando todos os arquivos {metaType}.meta...", ConsoleColor.Magenta);

            try
            {
                string targetDir = Path.Combine(GetDir(), $"{metaType}_meta");
                var files = Directory.GetFiles(sourceDirectory, $"{metaType}.meta", SearchOption.AllDirectories);

                for (int i = 0; i < files.Length; i++)
                {
                    string sourceFile = files[i];
                    string destFile = Path.Combine(targetDir, $"{metaType}{i}.meta");
                    File.Copy(sourceFile, destFile, true);
                }
                PrintColor($"Importação de todos os arquivos {metaType}.meta concluída! Foram encontrados {files.Length} arquivo(s).", ConsoleColor.Magenta);
            }
            catch (Exception ex)
            {
                PrintColor($"Erro durante a importação: {ex.Message}", ConsoleColor.Red);
            }
        }

        /// <summary>
        /// Importa arquivos com base em um padrão de busca (glob).
        /// </summary>
        private static void ImportFileByQueryFromDir()
        {
            string sourceDir = AskForPath("Caminho para o diretório de origem");
            if (sourceDir == null) return;

            string targetDir = AskForPath("Caminho para o diretório onde salvar os arquivos");
            if (targetDir == null) return;

            Console.Write("Consulta de pesquisa (ex: **/*.meta): ");
            string query = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(query))
            {
                PrintColor("A consulta de pesquisa não pode estar vazia.", ConsoleColor.Red);
                return;
            }

            try
            {
                // Simula a busca recursiva de '**/padrão'
                string searchPattern = Path.GetFileName(query);
                var files = Directory.GetFiles(sourceDir, searchPattern, SearchOption.AllDirectories);

                int filesCopied = 0;
                foreach (var file in files)
                {
                    string fileName = Path.GetFileName(file);
                    string destPath = Path.Combine(targetDir, fileName);
                    try
                    {
                        File.Copy(file, destPath, true);
                        filesCopied++;
                    }
                    catch (Exception ex)
                    {
                        PrintColor($"Ocorreu um erro ao copiar o arquivo:\nDe: {file}\nPara: {destPath}\n{ex.Message}", ConsoleColor.Red);
                    }
                }
                PrintColor($"Importação de todos os arquivos pela consulta: {query} concluída! Foram copiados {filesCopied} arquivo(s).", ConsoleColor.Magenta);
            }
            catch (Exception ex)
            {
                PrintColor($"Ocorreu um erro durante a busca pelos arquivos: {ex.Message}", ConsoleColor.Red);
            }
        }

        #region Funções Auxiliares

        /// <summary>
        /// Tenta analisar um arquivo XML. Se falhar, tenta uma correção robusta e analisa novamente.
        /// </summary>
        private static XDocument TryFixAndParseXml(string filePath)
        {
            string fileName = Path.GetFileName(filePath);
            try
            {
                string fileContent = File.ReadAllText(filePath);
                string sanitizedContent = SanitizeXmlString(fileContent);

                // Tentativa 1: Analisar diretamente o XML. A opção SetLineInfo ajuda a obter a linha do erro.
                try
                {
                    return XDocument.Parse(sanitizedContent, LoadOptions.SetLineInfo);
                }
                catch (System.Xml.XmlException ex)
                {
                    // Se falhar, inicia o processo de correção.
                    PrintColor($"Arquivo '{fileName}' inválido (Linha {ex.LineNumber}): Tentando correção automática...", ConsoleColor.Yellow);

                    var hapDoc = new HtmlDocument();
                    // Configurações agressivas para correção: consertar tags aninhadas e fechar tags automaticamente no final.
                    hapDoc.OptionFixNestedTags = true;
                    hapDoc.OptionAutoCloseOnEnd = true;
                    hapDoc.OptionOutputAsXml = true;

                    hapDoc.LoadHtml(sanitizedContent);

                    // Verifica se a HAP encontrou erros graves que não pôde corrigir.
                    // Ignoramos o erro "TagNotClosed" porque é exatamente isso que queremos que a HAP corrija.
                    var unfixableErrors = hapDoc.ParseErrors.Where(e => e.Code != HtmlParseErrorCode.TagNotClosed);
                    if (unfixableErrors.Any())
                    {
                        var errors = unfixableErrors.Select(e => $"  - {e.Reason} (Linha: {e.Line}, Código: {e.Code})");
                        string errorMsg = $"Não foi possível corrigir o arquivo '{fileName}'. Erros graves encontrados:\n{string.Join("\n", errors)}\n\n";
                        PrintColor(errorMsg, ConsoleColor.Red);
                        File.AppendAllText(Path.Combine(GetDir(), "errors.txt"), errorMsg);
                        return null;
                    }

                    string fixedXml;
                    using (var sw = new StringWriter())
                    {
                        hapDoc.Save(sw);
                        fixedXml = sw.ToString();
                    }

                    // Tentativa 2: Analisar o XML que foi corrigido pela HAP.
                    try
                    {
                        var correctedDoc = XDocument.Parse(fixedXml, LoadOptions.None);
                        PrintColor($"Arquivo '{fileName}' foi corrigido e analisado com sucesso!", ConsoleColor.Green);
                        return correctedDoc;
                    }
                    catch (System.Xml.XmlException finalEx)
                    {
                        // Se mesmo após a correção o XML ainda for inválido, o problema é mais sério.
                        string failedFilePath = Path.Combine(GetDir(), "output", $"FAILED_{fileName}");
                        string errorMsg = $"A correção automática para '{fileName}' falhou. O arquivo permanece inválido.\n  Erro final: {finalEx.Message} (Linha: {finalEx.LineNumber})\n  Uma tentativa de correção foi salva em: {failedFilePath}\n\n";
                        PrintColor(errorMsg, ConsoleColor.Red);
                        File.AppendAllText(Path.Combine(GetDir(), "errors.txt"), errorMsg);
                        File.WriteAllText(failedFilePath, fixedXml); // Salva a tentativa de correção para depuração
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMsg = $"Ocorreu um erro crítico ao processar o arquivo {fileName}\n{ex}\n\n";
                PrintColor(errorMsg, ConsoleColor.Red);
                File.AppendAllText(Path.Combine(GetDir(), "errors.txt"), errorMsg);
                return null;
            }
        }

        /// <summary>
        /// Obtém o diretório base da aplicação.
        /// </summary>
        private static string GetDir()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        /// <summary>
        /// Solicita um caminho de diretório ao usuário e valida sua existência.
        /// </summary>
        private static string AskForPath(string prompt)
        {
            Console.Write($"{prompt}: ");
            string path = Console.ReadLine()?.Trim('"'); // Remove aspas que podem ser coladas
            if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
            {
                return path;
            }
            else
            {
                PrintColor("O diretório não existe ou o caminho está vazio!", ConsoleColor.Red);
                return null;
            }
        }

        /// <summary>
        /// Imprime uma mensagem no console com uma cor específica.
        /// </summary>
        private static void PrintColor(string message, ConsoleColor color, bool newLine = true)
        {
            Console.ForegroundColor = color;
            if (newLine)
            {
                Console.WriteLine(message);
            }
            else
            {
                Console.Write(message);
            }
            Console.ResetColor();
        }

        /// <summary>
        /// Remove caracteres inválidos de uma string XML.
        /// </summary>
        private static string SanitizeXmlString(string xml)
        {
            if (string.IsNullOrEmpty(xml))
            {
                return xml;
            }

            StringBuilder buffer = new StringBuilder(xml.Length);
            foreach (char c in xml)
            {
                if (IsLegalXmlChar(c))
                {
                    buffer.Append(c);
                }
            }
            return buffer.ToString();
        }

        /// <summary>
        /// Verifica se um caractere é válido para XML 1.0.
        /// </summary>
        private static bool IsLegalXmlChar(int character)
        {
            return
            (
                 character == 0x9 /* == '\t' */   ||
                 character == 0xA /* == '\n' */   ||
                 character == 0xD /* == '\r' */   ||
                (character >= 0x20 && character <= 0xD7FF) ||
                (character >= 0xE000 && character <= 0xFFFD) ||
                (character >= 0x10000 && character <= 0x10FFFF)
            );
        }

        #endregion
    }
}



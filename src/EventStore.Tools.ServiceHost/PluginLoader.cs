using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using log4net;

namespace EventStore.Tools.ServiceHost
{
    internal static class PluginLoader
    {

        private static Assembly[] _assemblyCache;

        private static Assembly ResolveModuleAssembly(object sender, ResolveEventArgs args)
        {
            if (_assemblyCache == null)
            {
                _assemblyCache = AppDomain.CurrentDomain.GetAssemblies();
            }

            var asm = _assemblyCache.FirstOrDefault(a => a.FullName.StartsWith(args.Name));

            return asm;
        }

        private static void ValidateAssemblies(ILog logger)
        {
            try
            {
                var count = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                             from type in assembly.GetTypes()
                             select type).Count();

                //logger.InfoFormat("Checked {0} types..", count);

            }
            catch (ReflectionTypeLoadException ex)
            {
                logger.FatalFormat("Unable to load an assembly: {0}", ex.Message);

                foreach (var x in ex.LoaderExceptions)
                {
                    logger.FatalFormat("LoaderException: " + x.Message);
                }

                throw new Exception("Unable to load all types...");
            }
            catch (Exception ex)
            {
                logger.FatalFormat("Unable to load an assembly: {0}", ex.Message);
            }
        }

        private static void LoadReferencedAssemblies()
        {
            var asm = Assembly.GetExecutingAssembly();

            var currentAssemblies = new HashSet<string>((from a in AppDomain.CurrentDomain.GetAssemblies()
                                                         select a.GetName().Name), StringComparer.OrdinalIgnoreCase);

            RecurseReferencedAssemblies(asm, currentAssemblies);
        }

        private static void RecurseReferencedAssemblies(Assembly asm, ICollection<string> currentAssemblies)
        {
            foreach (var a in asm.GetReferencedAssemblies())
            {
                if (!currentAssemblies.Contains(a.Name))
                {
                    try
                    {
                        var loadedAsm = Assembly.Load(a.Name);
                        currentAssemblies.Add(a.Name);
                        RecurseReferencedAssemblies(loadedAsm, currentAssemblies);
                    }
                    catch { }
                }
            }
        }

        public static void LoadPlugins(string path, ILog logger, string currentDir = null)
        {
            logger.Info("Loading plugin assemblies");

            AppDomain.CurrentDomain.AssemblyResolve += ResolveModuleAssembly;

            var files = new List<string>();

            ValidateAssemblies(logger);

            LoadReferencedAssemblies();

            var currentAssemblies = new HashSet<string>((from a in AppDomain.CurrentDomain.GetAssemblies()
                                                         where !a.IsDynamic
                                                         select Path.GetFileName(a.Location).ToLower()));

            //var currentAssembliesDebug = (from a in AppDomain.CurrentDomain.GetAssemblies()
            //                              where !a.IsDynamic
            //                              select new
            //                              {
            //                                  File = Path.GetFileName(a.Location).ToLower(),
            //                                  Location = a.Location
            //                              });

            //foreach (var curr in currentAssembliesDebug)
            //{
            //    logger.InfoFormat("Current assembly: {0} ({1})", curr.File, curr.Location);
            //}

            var directories = new List<string> {path}; //paths.Plugins.ToList();

            if (currentDir != null)
            {
                directories.Insert(0, currentDir);
            }

            var shadowCopyStr = ConfigurationManager.AppSettings["ShadowCopy"];
            var shadowCopy = true;

            if (!string.IsNullOrEmpty(shadowCopyStr))
            {
                shadowCopy = bool.Parse(shadowCopyStr);
            }

            foreach (var plugin in directories)
            {
                if (!Directory.Exists(plugin))
                {
                    logger.InfoFormat("Directory {0} does not exist, skipping", plugin);
                    continue;
                }

                var configFile = Path.Combine(plugin, "plugins.config");

                HashSet<string> filesToLoad = null;

                //if (!File.Exists(configFile))
                //{
                //    logger.InfoFormat("No plugins.config present in {0}", plugin);
                //}
                //else
                if (File.Exists(configFile))
                {
                    filesToLoad = new HashSet<string>(File.ReadAllLines(configFile), StringComparer.OrdinalIgnoreCase);
                }

                logger.InfoFormat("Loading plugins in {0}..", plugin);

                foreach (var file in Directory.GetFiles(plugin, "*.dll"))
                {
                    var name = Path.GetFileName(file).ToLower();
                    if (!currentAssemblies.Contains(name) && (filesToLoad == null || filesToLoad.Contains(name)))
                    {
                        files.Add(file);
                        currentAssemblies.Add(name);
                        logger.InfoFormat("Going to load {0} ({1})", name, file);
                    }
                }
            }

            foreach (string file in files)
            {
                try
                {
                    if (shadowCopy)
                    {
                        var tempFile = Path.GetTempFileName();
                        File.Copy(file, tempFile, true);
                        var asm = Assembly.LoadFile(tempFile);

                        logger.InfoFormat("Loaded assembly {0}..", asm.FullName);
                    }
                    else
                    {
                        var asm = Assembly.LoadFile(file);

                        logger.InfoFormat("Loaded assembly {0}..", asm.FullName);
                    }
                }
                catch (ReflectionTypeLoadException ex)
                {
                    logger.ErrorFormat("ReflectionTypeLoadException on trying to load assembly {0}: {1}", file, ex.Message + Environment.NewLine + ex.StackTrace);

                    foreach (var x in ex.LoaderExceptions)
                    {
                        logger.ErrorFormat("LoaderException: " + x.Message);
                    }
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Exception on trying to load assembly {0}: {1}", file, ex.Message + Environment.NewLine + ex.StackTrace);
                }
            }

            ValidateAssemblies(logger);
        }
    }
}

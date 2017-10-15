using log4net;
using log4net.Appender;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using System.IO;

namespace ExternalData.Extensions
{
    public static class Log4NetExtensions
    {
        public static void AddFile(this ILog log, string name, string root, string extension, string pattern, string level)
        {
            Logger logger = (Logger)log.Logger;
            logger.Level = logger.Hierarchy.LevelMap[level];

            PatternLayout layout = new PatternLayout { ConversionPattern = pattern };

            FileAppender appender =
                new FileAppender
                {
                    Name = name + "Appender",
                    File = Path.Combine(root, string.Format("{0}.{1}", name, extension)),
                    AppendToFile = false,
                    Layout = layout
                };

            layout.ActivateOptions();
            appender.ActivateOptions();

            logger.AddAppender(appender);
        }
    }
}

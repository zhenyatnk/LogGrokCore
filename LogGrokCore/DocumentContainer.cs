﻿using DryIoc;
using LogGrokCore.Data;
using System;
using System.Text.RegularExpressions;
using LogGrokCore.Data.Virtualization;

namespace LogGrokCore
{
    public enum ParserType
    {
        Full,
        OnlyIndexed
    }

    internal class DocumentContainer : IDisposable
    {
        private readonly Container _container;

        public DocumentContainer(string fileName)
        {
            var regex =
                @"^(?'Time'\d{2}\:\d{2}\:\d{2}\.\d{3})\t(?'Thread'0x[0-9a-fA-F]+)\t(?'Severity'\w+)\t(?'Component'\w+)?\t(?'Message'.*)";
            _container = new Container(rules =>
                rules
                    .WithDefaultReuse(Reuse.Singleton)
                    .WithUnknownServiceResolvers
                        (Rules.AutoResolveConcreteTypeRule()));

            _container.Register<Loader>();
            
            _container.Register<LineIndex>();
            _container.Register<IItemProvider<string>, LineProvider>();

            _container.RegisterDelegate<ILineParser>(
                r => new RegexBasedLineParser(r.Resolve<LogMetaInformation>(), true), 
                serviceKey: ParserType.OnlyIndexed);

            _container.RegisterDelegate<ILineParser>(
                r => new RegexBasedLineParser(r.Resolve<LogMetaInformation>()), 
                serviceKey: ParserType.Full);
            
            _container.Register<ILineDataConsumer, LineProcessor>(
                made: Parameters.Of.Type<ILineParser>(serviceKey: ParserType.OnlyIndexed));
            
            _container.Register<DocumentViewModel>(
                made: Parameters.Of.Type<ILineParser>(serviceKey: ParserType.Full));

            _container.Register<StringPool>(Reuse.Singleton);
            
            _container.RegisterDelegate(r => new LogFile(fileName));
            _container.RegisterDelegate(r => new LogMetaInformation(regex, new[] {1, 2, 3}));
        }

        public DocumentViewModel GetDocumentViewModel() => _container.Resolve<DocumentViewModel>(); 

        public void Dispose() => _container.Dispose();
    }
}
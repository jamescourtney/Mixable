﻿namespace ConfiguratorDotNet.Runtime;

public class StringScalarParser : IScalarParser
{
    public bool CanParse(string value)
    {
        return value is not null;
    }
}
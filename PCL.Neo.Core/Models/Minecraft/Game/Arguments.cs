
        var jvmArgu = arguments.Jvm
            .Where(it => it.Rules is null || JvmArgumentsFilter(it.Rules.FirstOrDefault()!))
            .SelectMany(it => it.Value);

        var gameResult = ReplaceCustomValue(gameArgu, options); // todo: add jvm argu and -D argu
        var jvmResult = ReplaceCustomValue(jvmArgu, options);

        GameArguments = gameResult.Concat(jvmResult).ToList();
    }

    /// <inheritdoc />
    public override string ToString() =>
        string.Join(" ", GameArguments);

    [GeneratedRegex(@"\$\{([^}]+)\}")]
    private static partial Regex CustomValueRegex();
}
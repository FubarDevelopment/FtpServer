﻿// This file isn't generated, but this comment is necessary to exclude it from StyleCop analysis.
// <auto-generated/>

using DotNet.Globbing.Token;

namespace DotNet.Globbing
{
    internal interface IGlobTokenVisitor
    {
        void Visit(WildcardToken token);
        void Visit(WildcardDirectoryToken wildcardDirectoryToken);
        void Visit(AnyCharacterToken token);
        void Visit(LetterRangeToken token);
        void Visit(NumberRangeToken token);
        void Visit(CharacterListToken token);
        void Visit(LiteralToken token);
        void Visit(PathSeperatorToken token);

    }
}
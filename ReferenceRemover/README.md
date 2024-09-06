ReferenceRemover
================

Removes references from an assembly. This is handy when using ILMerge.

Requires: Mono.Cecil

Usage: ReferenceRemover Assembly [Regex [ExcludeRegex] Target]

If Regex is omitted, the references are listed.

Example: ReferenceRemover IronScheme.Web.Runtime.dll "IronScheme\..*|Oyster.IntX.*" IronScheme.dll

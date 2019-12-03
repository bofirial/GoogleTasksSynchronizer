// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Reliability", 
    "CA2007:Consider calling ConfigureAwait on the awaited task", 
    Justification = "No UI Thread to Protect")]

[assembly: SuppressMessage("Usage", 
    "CA2227:Collection properties should be read only", 
    Justification = "Collection Properties are assigned to during data lookups")]

[assembly: SuppressMessage("Globalization", 
    "CA1303:Do not pass literals as localized parameters", 
    Justification = "Application is not locallized")]

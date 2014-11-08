// Guids.cs
// MUST match guids.h
using System;

namespace Alphaleonis.Horseshoe
{
   static class GuidList
   {
      public const string guidHorseshoePkgString = "213A4B88-7720-4661-A07F-FEC20B51D13B";
      public const string guidHorseshoeCmdSetString = "22a959e1-1639-4a76-adea-6d11be0cc7ba";

      public static readonly Guid guidHorseshoeCmdSet = new Guid(guidHorseshoeCmdSetString);
   };
}
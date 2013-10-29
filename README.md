XsdDownloader
=============

A simple command line utility for recursively downloading included and imported XSD schemas for use with Visual Studio's xsd.exe xsd-to-code functionality.

This program will take online XML schemas (XSD), download them to a local file, as well as also recursively downloading any XSDs that they reference using &lt;include&gt; and &lt;import&gt; tags.

xsd.exe will automatically process *.xsd files that are &lt;include&gt;d (as long as they are in the same folder), but &lt;import&gt;ed XSDs need to be specified on the command line. XsdDownloader takes care of this by tracking which files were &lt;import&gt;ed and generating a corresponding command line for xsd.exe in `create_classes_from_xsd.bat` in the output directory.

Requirements
------------
.NET Framework 3.5

Usage examples
--------------
Download a single XSD (and its includes and imports) to the current directory:

    XsdDownloader --input=http://example.com/XMLSchemas/schema1.xsd

Download multiple XSDs to a specified directory:

    XsdDownloader --input=http://example.com/XMlSchemas/schema1.xsd;http://example.com/XMLSchemas/schema2.xsd --output=C:\XSD

You can also set the `--namespace` argument for the classes that will be generated; this does not affect XSD downloading and is passed as-is to the `/namespace:` argument of xsd.exe (in the `create_classes_from_xsd.bat` file).

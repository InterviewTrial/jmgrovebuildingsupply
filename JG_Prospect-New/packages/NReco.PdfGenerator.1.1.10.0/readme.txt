NReco.PdfGenerator (wkhtmltopdf wrapper)
----------------------------------------
Visit http://www.nrecosite.com/pdf_generator_net.aspx for the latest information (change log, examples, etc)
API documentation: http://www.nrecosite.com/doc/NReco.PdfGenerator/
Embedded wkhtmltopdf: v.0.12.2.1 (x86 MSVC++ 2013 build, VC++ runtime libs are bundled) 

How to use:
	var pdfBytes = (new NReco.PdfGenerator.HtmlToPdfConverter()).GeneratePdf(htmlContent);
(see GeneratePdf and GeneratePdfFromFile overloads for more options)

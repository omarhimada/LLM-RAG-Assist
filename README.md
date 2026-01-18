# LLM-RAG-Assist

## Trim unnecessary parts of PDF and EPUB before feeding them into your LLM/RAG system
- LLM-RAG-Assist provides tools to trim PDF documents and EPUB within a given page range, 

- A lot of businesses/organizations/individuals are uploading things like entire PDFs or EPUBs into their LLM/RAG systems without trimming out the irrelevant parts. 
    + For example, the '*introduction*', '*about the author*', '*index*', and other prefacing or ending sections.

- Without trimming the irrelevant sections you're feeding your model little pieces of garbage while, probably, paying for your LLM system to process irrelevant information which is a waste of resources and money.

### Includes a utility to convert EPUB to PDF
- Convert the EPUB to PDF if your LLM model/system you're using only supports PDFs but you purchased an e-book and they gave you EPUB, for example.

![NuGet Version](https://img.shields.io/nuget/v/LLM-RAG-Assist?style=flat)

# Nexus.Link.Libraries

All libraries currently support both .NET Standard (and thereby .NET Core) and .NET Framework.

### Nexus.Link.Libraries.Core

Core functionality that only depends on Microsoft libraries and Newtonsoft.Json. This is where you find things like application setup, error handling and logging.

See the [Documentation](src/Libraries.Core/README.md).

### Nexus.Link.Libraries.Web

REST functionality that is not dependent on neither ASP.NET WebApi, nor ASP.NET Core WebApps, i.e. functionality for outgoing REST calls. Examples of what it provides is handling of CorrelationId, logging HTTP requests and converting unsuccessful HTTP responses into relevant exceptions.

See the [Documentation](src/Libraries.Web/README.md).

### Nexus.Link.Libraries.Web.AspNet

Functionality for providing ASP.NET WebApi (based on .NET Framework) or ASP.NET Core WebApps (based on .NET Core), i.e. functionality for incoming REST calls.

### Nexus.Link.Libraries.Crud

Interfaces ("ICrud") and memory implementations of CRUD operations. Excellent for creating storage mocks in no time and sets the ground for other libraries that support ICrud.

### Nexus.Link.Libraries.Crud.WebApi

Makes it really easy to call REST API:s that are based on ICrud.

### Nexus.Link.Libraries.Crud.AspNet

With this functionality, you can write ICrud API:s with very little code and you can have a mock service up in no time.

### Nexus.Link.Libraries.SqlServer

Functionality for accessing data on a Microsoft SQL Server. Supports ICrud, so it is really easy to replace your storage mocks with storage in a relational database.

### Nexus.Link.Libraries.Azure

Functionality for accessing storage, queues and more in Azure. Supports ICrud, so that it is really easy to replace your storage mocks with storage in the cloud.

## License

All libraries are provided under the [MIT license](https://choosealicense.com/licenses/mit/):

```
MIT License

Copyright (c) 2018 Nexus Link AB

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```

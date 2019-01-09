# HTML2PDF-netcore

> Generating editable PDF files from HTML code with CSS properties support

## Dependency

- itext7
  - Version: 7.1.4
  - Author: iText Software
  - license: [AGPL](http://www/gnu.org/licenses/agpl.html)
- itext7.pdfhtml
  - Version: 2.1.1
  - Author: iText Software
  - license: [Legal](http://itextpdf.com/legal)

## Getting Start

- [Install the .NET SDK](https://dotnet.microsoft.com/learn/dotnet/hello-world-tutorial)

- Dependency Reference

```pm
pm> Install-Package itext7 -Version 7.1.4
pm> Install-Package itext7.pdfhtml -Version 2.1.1
```

- Run App

```linux
~$ dotnet HTML2PDF-netcore.dll
```

- API
  - [Create](http://localhost:5000/api/pdf/create?pdfHtmlString=&saveName=)
    - uri: http://localhost:5000/api/pdf/create?pdfHtmlString=&saveName=
    - method: GET
    - params:

        |      name     |  type  | meaning |
        | ------------- | ------ | ------- |
        | pdfHtmlString | string | HTML document in string     |
        | saveName      | string | The name of PDF to be saved |

    - Example:

    `http://localhost:5000/api/pdf/create?pdfHtmlString=<div%20class="main"%20style="margin:%2030px%200;">DHCPスヌーピングをサポートし、DHCPサーバを設定し、DHCPサーバの適合性を保証します。</div>&saveName=test`

  - [TestCreate](http://localhost:5000/api/pdf/testcreate)

    > The test method for API [Create](http://localhost:5000/api/pdf/create?pdfHtmlString=&saveName=). You will get a pdf created using the existed HTML string(parameter `pdfHtmlString` in [Create](http://localhost:5000/api/pdf/create?pdfHtmlString=&saveName=)) as follows by calling this.

    ```html
    <div class="main" style="margin: 30px 0;">
      DHCPスヌーピングをサポートし、DHCPサーバを設定し、DHCPサーバの適合性を保証します。DoS防御をサポートし、防御ランドスキャン、SYNFIN、Xmascan、Ping Floodingなどを攻撃。
    </div>
    ```

    - uri: http://localhost:5000/api/pdf/testcreate
    - method: GET
    - If you are using a machine with Unix system, the generated PDF file can be found at path `/data/webroot/pdf/`.
    - If you are using a machine with Windows system, the generated PDF file **may** be found at path `D:\pdf`(If disk `D` exist).

## Declaration

- This project is license by [GNU GENERAL PUBLIC LICENSE](LICENSE).
- The ownership of this project is owned by the [author](https://github.com/RyougiChan). All resources in this project are based on [CC BY-NC-SA 4.0 ](https://creativecommons.org/licenses/by-nc-sa/4.0/), that means  you can copy and reissue the contents of this project, but you will also have to provide the **original author information** as well as the **agreement statement**. At the same time, it **cannot be used for commercial purposes**. In accordance with our narrow understanding (Additional subsidiary terms), **All activities that are profitable are of commercial use**.
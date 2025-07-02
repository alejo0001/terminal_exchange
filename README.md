# CRM

## Descripción

Esto es el CRM. Sirve para gestion procesos de venta relacionados con Contactos.

### .Net

El proyecto usa C# y la plataforma .Net Framework 5.0.

Pendiente de migrar a 8.0

## Recursos

## Features 🚩
- [Endpoint](./docs/endpoints/README.md)

### Bases de datos

*Test
*Servidor: db-intranet-test
*BD: db-intranet-test
*Esquema: dbo

*Servidor: db-cursos-enfermeria-net-prod
*BD: aula-salud_dev2
*Esquema: mysql


*Produccion
*Servidor: db-intranet
*BD: db-intranet
*Esquema: dbo

*Servidor: db-cursos-enfermeria-net-prod
*BD: aula-salud
*Esquema: mysql

## Package management (🛠️ _TODO: upgrade pack-publish tooling to v2 and update Readme too_)
#### Currently only goes for Contracts project 🪧
### Package publishing procedure
... After doing all the changes

1. **Increment the version**: open the file [Directory.Build.props](Directory.Build.props) and rise version accordingly:
    1. braking changes => increment major and set minor & patch to 0;
    2. backwards compatible changes, new features/models, that do not break existing functionality => increment minor and set patch to 0;
    3. non-breaking changes to existing model, that do not break existing functionality => increment patch;
    4. documentation, chores, any changes that don't touch model => add/increment "B" in `x.y.z.B`.
    5. NB! If you want to publish a package for testing, you can add "alpha, beta, preview" labels to any positions to mark them as *PreRelease packages*; NuGet package manager can opt-out PreRelease packages so that compañeros can avoid stumbling to them if they happen to update their projects during the testing period.

2. **Publish the package** with the following command (script takes care of packing), you have two options:
    1. uses version from [Directory.Build.props](Directory.Build.props):
       ```
       pwsh -executionpolicy bypass -File .\RunPublishWorkflow.ps1 [-ToLocalSource]
       ```
    2. allows to override package version:
       ```
       pwsh -executionpolicy bypass -File .\RunPublishWorkflow.ps1 --PublishVersionOverride x.y.z [-ToLocalSource]
       ```
    * There is an optional switch `-ToLocalSource` that will automatically configure Nuget Local Source named `nuget-local-source-tech`, that resides at the paren folder of the solution's folder. Idea is to provide convenient local DX when working with Contract source and its consume solutions. Just open NuGet package manager and seek for NuGet feed with this name. This mode don't publish anything to repository.
    * IntelliJ IDEs can also benefit from ready-made task runners, to execute normal or `-ToLocalSource` mode publish.

Reason to use the file [Directory.Build.props](Directory.Build.props) is to manage version allows more flexibility in case more projects will be added to the solution.

### Pushing code to repository
It would be nice to have a habit to add Git the tag that corresponds to version value from [Directory.Build.props](Directory.Build.props).
It allows to quickly match package version with their corresponding commits so package consumers can track down some release notes-like info a bit quicker.

# Emergency change to induce API instances restart via deploy. 🚩

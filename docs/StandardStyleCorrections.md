# Correcciones del punto 4 del estándar

Fecha: 3 de octubre de 2026.

Se corrigió el código del proyecto que integra `DominoTrainGame.slnx`, tomando como referencia
`EstandarTecnologias-Equipo 4.pdf`. La revisión abarca los 63 archivos C#, las 20 vistas y diccionarios XAML,
las dos plantillas T4 y el código generado que participa en la compilación. Se añadió una configuración
del editor y un script para conservar el formato del código generado.

Este cambio corresponde a nombres, archivos, importaciones, namespaces, formato, organización de miembros,
comentarios y pendientes. No agrega una suite de pruebas ni define los futuros componentes del servidor.
El informe de auditoría anterior corresponde al estado previo a estas correcciones.

## Cambios aplicados

- Propiedades del modelo en PascalCase: `Username`, `FriendshipRequestId`, `ReportId` y `StatusId`.
- Relaciones de amistad con nombres descriptivos: `AddresseePlayer`, `RequesterPlayer`,
  `ReceivedFriendshipRequests` y `SentFriendshipRequests`.
- Archivos alineados con los tipos que contienen y referencias del proyecto actualizadas.
- Importaciones innecesarias eliminadas, con un bloque ordenado: System, bibliotecas externas y proyecto.
- Namespaces de ámbito de archivo; llaves Allman y llaves explícitas en los controles de flujo.
- Indentación de cuatro espacios y un máximo de 120 columnas en C#, XAML, T4 y el script de mantenimiento.
- Miembros ordenados por categoría y líneas en blanco corregidas, conservando la agrupación legible del código.
- Veintiocho manejadores de eventos renombrados, con sus referencias XAML y suscripciones actualizadas.
  Los parámetros de los eventos utilizan `sender` y `e`.
- Campos privados del código generado con prefijo `_`; comentarios del equipo en inglés y sin texto obsoleto.
- Pendientes descritos mediante `TODO` específicos. El `FIXME` sin explicación fue sustituido por un comentario
  sobre la inicialización de los paneles, y su condición recibió las llaves correspondientes.

### Archivos renombrados

| Archivo anterior | Archivo actual |
| --- | --- |
| `Server/Services/Authentication/RecoveryPasswordValidator.cs` | `Server/Services/Authentication/RecoverPasswordValidator.cs` |
| `Shared/Contracts/Authentication/RecoveryPasswordValidationStatus.cs` | `Shared/Contracts/Authentication/RecoverPasswordValidationStatus.cs` |
| `Server/Data/DominoGameDBEntities.Configuration.cs` | `Server/Data/DominoGameDBEntities.cs` |
| `Server/Data/EntityFramework/DominoGameModel.Context.Context.cs` | `Server/Data/EntityFramework/DominoGameDBEntities.cs` |
| `Server/Data/EntityFramework/DominoGameModel.Context.Context.tt` | `Server/Data/EntityFramework/DominoGameDBEntities.tt` |
| `scripts/sql/habilitar_modo_mixto.sql` | `scripts/sql/enable_mixed_authentication.sql` |

Las rutas C# de la tabla son relativas a `scr`. Las dos partes de `DominoGameDBEntities` mantienen su carácter
parcial y cada archivo coincide con el nombre del tipo en su carpeta correspondiente.

## Compatibilidad del modelo

Los nombres corregidos son los del modelo conceptual y sus propiedades C#. Las columnas físicas se conservan
mediante `ColumnName` en el EDMX: `userName`, `FriendshipRequestsID`, `idReports`, `idStatus` e `IdStatus`.
Se verificó que todo el esquema de almacenamiento y todos los nombres de columnas de los mapeos permanecen iguales.
No se ejecutó una migración ni se modificó una base de datos.

El perfil del archivo de configuración de ejemplo se llama `Teammate`. Las configuraciones privadas existentes
conservan sus valores. Las cadenas de conexión del XML mantienen sus valores y no se consideran líneas de código C#.

## Conservación del formato

`.editorconfig` define el formato y las convenciones de nombres para nuevas ediciones.
`scripts/NormalizeGeneratedCode.ps1` normaliza las salidas de Entity Framework, recursos y configuración antes
de compilar. El proyecto lo ejecuta automáticamente, de manera que la regeneración no restaure los nombres
de campos privados, los namespaces con bloque ni los comentarios antiguos de los diseñadores.

Para cambiar nombres del modelo se debe editar el EDMX conceptual y regenerar las plantillas T4.
Los comentarios `TODO` de perfiles, amistades y enlaces sociales deben resolverse cuando se definan esas funciones.
Su presencia describe el estado actual de desarrollo; no implica que esas funciones estén terminadas.

## Verificación realizada

| Comprobación | Resultado |
| --- | --- |
| Compilación Debug, .NET Framework 4.8, AnyCPU | Correcta; sin errores. |
| Compilación Release, .NET Framework 4.8, AnyCPU | Correcta; sin errores. |
| Revisión de columnas, indentación, espacios, namespaces y marcadores | Sin incidencias en el alcance indicado. |
| Análisis semántico de importaciones | Ningún `using` innecesario en los 63 archivos C# del proyecto. |
| Referencias XAML y valores de diseño | Conservados; los manejadores renombrados están enlazados. |
| Inicialización de las ventanas | Las 16 ventanas se crearon en memoria sin mostrarlas ni solicitar datos. |
| Validación de Entity Framework | Modelo conceptual, almacenamiento, mapeos y clases CLR válidos; sin abrir SQL. |
| Regeneración de las plantillas | Once salidas del modelo coinciden con los tokens C# del proyecto. |
| Regeneración de recursos y configuración | Las tres salidas conservan los tokens C# corregidos. |
| Normalización repetida del código generado | Idempotente: una segunda ejecución no cambia los archivos. |

La ejecución de las plantillas se comprobó en una carpeta temporal con las herramientas de Visual Studio.
Para utilizar el ejecutor fuera del editor se omitió, solo en la copia temporal del archivo de inclusión de Microsoft,
su directiva de limpieza específica del host de Visual Studio. Se conservó el archivo instalado y las plantillas del proyecto.

Permanece la advertencia previa `CS8632` en `LoginValidator`: una anotación de referencia nullable aparece en
un archivo sin contexto de nulabilidad habilitado. Su corrección corresponde al punto de nulabilidad del informe original.
La validación de carga reutilizó la redirección de versión de log4net que ya contiene la configuración de la aplicación.

Las comprobaciones anteriores verifican la integridad de este cambio. No sustituyen las futuras pruebas funcionales,
de integración y del servidor, ni declaran resueltos los demás puntos del informe de auditoría.

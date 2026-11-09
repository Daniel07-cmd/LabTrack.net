# Despliegue en MonsterASP.NET (.NET 10 + MSSQL)

Esta guía deja el sistema **LabTrack.Web** publicado en MonsterASP.NET usando
ASP.NET Core 10, SQL Server y HTTPS.

## 1. Requisitos en el panel de MonsterASP.NET

1. Cree un **sitio web** (Website) con runtime **.NET 10**.
2. Cree una **base de datos MSSQL**.
3. Active **HTTPS** en el sitio (Let's Encrypt).
4. En la base de datos, abra **Users → Connection string** y copie la cadena
   **Local access** (no la remota). Se ve así:

   ```text
   Server=dbXXXX.databaseasp.net; Database=dbXXXX; User Id=dbXXXX; Password=...; Encrypt=False; MultipleActiveResultSets=True;
   ```

   > La cadena **Local** es la que usa el sitio dentro de la red de MonsterASP.
   > La remota solo sirve para conectarse desde su PC y requiere habilitar el
   > acceso remoto.

## 2. Configurar la cadena de conexión

El proyecto **no** guarda credenciales en el repositorio. En producción se lee
de la variable de entorno:

```text
ConnectionStrings__LabTrackConnection
```

Configúrela en el panel de MonsterASP.NET del sitio (sección de variables de
entorno / app settings) con la cadena **Local** del paso 1.

Alternativa: editar `appsettings.Production.json` antes de publicar y pegar la
cadena en `ConnectionStrings:LabTrackConnection`. Si usa esta opción, no suba
ese archivo a un repositorio público.

Si la cadena falta o es incorrecta, la aplicación muestra un error claro al
arrancar y registra el detalle en los logs.

## 3. Generar el paquete

Desde la carpeta del proyecto:

```powershell
.\publish.ps1 -Zip
```

> Si PowerShell bloquea la ejecución de scripts, use:
> `powershell -ExecutionPolicy Bypass -File .\publish.ps1 -Zip`

Esto crea:

- `publish\` → contenido listo para subir.
- `LabTrack.Web.zip` → el mismo contenido comprimido.

La publicación es **framework-dependent** (no incluye el runtime de .NET), que
es lo recomendado en MonsterASP porque el runtime .NET 10 ya está instalado en
el servidor.

## 4. Subir el sitio

Elija una de estas opciones:

### Opción A — ZIP (la más rápida)

1. En MonsterASP abra **Files** del sitio.
2. Suba `LabTrack.Web.zip` y extráigalo **dentro de `\wwwroot`**.
3. Verifique que `web.config` quede directamente en `\wwwroot\web.config`.
4. Reinicie el sitio desde **Overview → Restart**.

### Opción B — FTP / SFTP

1. Abra **Deploy → FTP / SFTP access** y copie servidor, puerto, usuario y
   contraseña.
2. Conecte con FileZilla (prefiera FTPS/SFTP).
3. Suba **el contenido** de `publish\` dentro de `\wwwroot`.
   No suba la carpeta `publish` como subcarpeta.

### Opción C — Web Deploy desde Visual Studio

1. En **Deploy → Visual Studio access**, habilite **WebDeploy** y descargue el
   archivo `.publishSettings`.
2. En Visual Studio: clic derecho al proyecto → **Publish** → **Import profile**.
3. En **Show all settings → Databases / Entity Framework Migrations**, pegue la
   cadena **Local** de MSSQL (la misma del paso 1).
4. Publique.

> Este proyecto aplica las migraciones automáticamente al arrancar, así que la
> sección de EF Migrations del asistente es opcional.

### Opción D — GitHub deploy

1. Suba el proyecto a GitHub (el `.gitignore` ya excluye `bin`, `obj`, `publish`
   y secretos).
2. En MonsterASP: **Git deploy → Build .NET app**.
3. Seleccione el `.csproj` `LabTrack.Web/LabTrack.Web.csproj`.
4. Configure la variable de entorno del paso 2 y publique.

## 5. Primer arranque

Al iniciar, la aplicación:

1. Aplica las migraciones pendientes de EF Core.
2. Crea usuarios y equipos de ejemplo **solo si la base está vacía**.

Usuarios iniciales:

| Rol | Código | Contraseña |
|---|---|---|
| Encargado | `ENC001` | `Admin123*` |
| Estudiante | `EST001` | `Estudiante123*` |

> Cambie estas contraseñas o elimine los usuarios de prueba antes de usar el
> sistema con datos reales.

## 6. Verificación rápida

1. Abra `https://su-sitio.siteasp.net/`.
2. Inicie sesión con `ENC001 / Admin123*`.
3. Compruebe **Dashboard**, **Equipos**, **Reservas**.
4. Cierre sesión e inicie con `EST001 / Estudiante123*`.
5. Compruebe **Catálogo** y solicite una reserva.

## 7. Solución de problemas

### HTTP Error 500.30 - ASP.NET Core app failed to start

Este error significa que el proceso de la aplicación no llegó a arrancar. A partir
de esta versión, si el problema es de configuración o de base de datos, la
aplicación **sí arranca** y muestra una página de diagnóstico en:

```text
https://su-sitio.siteasp.net/diagnostico
```

Si al entrar a `/diagnostico` ves el detalle del error, corrige lo que indica y
reinicia el sitio. Si **sigue apareciendo el 500.30** incluso en `/diagnostico`,
el fallo es anterior a la aplicación; revisa en este orden:

1. **Runtime del sitio**: en MonsterASP, el sitio debe estar en **.NET 10**
   (Overview → versión de .NET). Si está en .NET 8/9, cámbialo y reinicia.
2. **Ubicación de archivos**: en `wwwroot` deben estar directamente
   `web.config`, `LabTrack.Web.dll` y `appsettings.json`. No deben quedar dentro
   de una subcarpeta tipo `wwwroot\publish\`.
3. **Cadena de conexión**: debe existir
   `ConnectionStrings__LabTrackConnection` con la cadena **Local access**, o
   estar escrita en `appsettings.Production.json`.
4. **Log de arranque**: revisa `wwwroot\logs\stdout_*.log`. Ahí aparece la
   excepción exacta que impidió arrancar. También puedes revisar los logs del
   panel de MonsterASP.

Si el log dice algo como `The framework 'Microsoft.AspNetCore.App', version
'10.0.0' was not found`, el sitio no está configurado en .NET 10.

### Error 500 al arrancar

Casi siempre es la cadena de conexión. Revise:

- Que la variable `ConnectionStrings__LabTrackConnection` exista y tenga la
  cadena **Local** (no la remota).
- Que el usuario de la base tenga permisos para crear tablas.
- Los logs del sitio en MonsterASP (**Logs** / `logs\stdout`).

### La aplicación no conecta a la base

- Use el host `dbXXXX.databaseasp.net` que aparece en el panel.
- Mantenga `Encrypt=False` para la cadena local.
- No use `localhost` ni `SQLEXPRESS` en producción: esa cadena es solo para
  desarrollo.

### La sesión se pierde

Las sesiones están en memoria (`AddDistributedMemoryCache`). Si el app pool se
recicla, los usuarios deben volver a iniciar sesión. Es normal en este tipo de
hosting; si necesita persistencia, cambie a sesiones distribuidas en SQL Server.

### HTTPS / redirecciones

Active HTTPS en el panel del sitio. La aplicación ya configura
`ForwardedHeaders` para funcionar detrás de IIS/proxy.

## 8. Notas de seguridad

- Las contraseñas nuevas se guardan con **PBKDF2-SHA256**. Las contraseñas
  antiguas en texto plano se migran automáticamente al iniciar sesión.
- Los formularios usan **AntiForgeryToken**.
- El CRUD de equipos y la bandeja de reservas están restringidos al rol
  `Encargado`.
- No publique `appsettings.Production.json` con credenciales reales en un
  repositorio público.

\# GAC Condominium



Sistema web para la gestión, administración y control de un condominio residencial.



GAC Condominium fue desarrollado como una solución para centralizar la administración de unidades habitacionales, residentes, pagos, cuotas de mantenimiento, contratos de alquiler, reservas de áreas comunes, incidencias, documentos, avisos y reportes administrativos.



\## 📌 Descripción



El sistema está orientado a la gestión de un condominio residencial y permite administrar diferentes procesos relacionados con la operación diaria del mismo.



La aplicación cuenta con interfaces diferenciadas para \*\*Administradores\*\* e \*\*Inquilinos\*\*, proporcionando a cada tipo de usuario acceso a las funcionalidades correspondientes.



\## ⚙️ Funcionalidades principales



\### Administración



\* Gestión de condominios.

\* Gestión de unidades habitacionales.

\* Asignación de residentes a unidades.

\* Gestión de usuarios y roles.

\* Gestión de contratos de alquiler.

\* Gestión de cuotas de mantenimiento.

\* Registro y gestión de pagos.

\* Gestión de áreas comunes.

\* Reservación de áreas comunes.

\* Gestión de incidencias de mantenimiento.

\* Gestión de documentos.

\* Publicación de avisos.

\* Gestión de solicitudes de ingreso.

\* Generación de reportes administrativos y financieros.

\* Dashboard administrativo con información general del condominio.



\### Inquilinos



\* Visualización de información de su unidad.

\* Consulta de pagos y deudas.

\* Historial de pagos.

\* Consulta de avisos.

\* Consulta y gestión de reservas.

\* Reporte de incidencias de mantenimiento.

\* Consulta de documentos relacionados.

\* Visualización de información relevante del condominio.



\## 👥 Roles



\### Administrador



Tiene acceso a las funcionalidades administrativas del sistema, incluyendo la gestión de:



\* Usuarios

\* Unidades

\* Residentes

\* Pagos

\* Cuotas

\* Contratos

\* Áreas comunes

\* Reservas

\* Incidencias

\* Documentos

\* Avisos

\* Reportes

\* Solicitudes de ingreso



\### Inquilino



Dispone de una interfaz enfocada en la consulta y gestión de información relacionada con su unidad y su relación con el condominio.



\## 🛠️ Tecnologías utilizadas



\* \*\*C#\*\*

\* \*\*ASP.NET Core MVC\*\*

\* \*\*Entity Framework Core\*\*

\* \*\*SQL Server\*\*

\* \*\*Razor Views\*\*

\* \*\*Bootstrap 5\*\*

\* \*\*HTML5 / CSS3\*\*

\* \*\*JavaScript\*\*

\* \*\*MailKit\*\*

\* \*\*.NET User Secrets\*\*



\## 🏗️ Arquitectura



El proyecto utiliza el patrón \*\*Model-View-Controller (MVC)\*\* proporcionado por ASP.NET Core.



```text

PROTOTIPO/

│

├── Controllers/

├── Helpers/

├── Models/

│   └── ViewModel/

├── Services/

├── Views/

│   ├── Account/

│   ├── Admin/

│   ├── Inquilino/

│   ├── PagosInquilino/

│   ├── Reportes/

│   ├── ReservasInquilino/

│   └── Shared/

│

├── wwwroot/

│   ├── css/

│   ├── js/

│   └── lib/

│

├── Program.cs

├── PROTOTIPO.csproj

├── appsettings.json

└── appsettings.Development.json

```



\## 🗄️ Base de datos



El sistema utiliza \*\*SQL Server\*\* como sistema gestor de base de datos y \*\*Entity Framework Core\*\* para la interacción entre la aplicación y la base de datos.



El proyecto utiliza un enfoque \*\*Database First\*\*, donde el modelo de datos de la aplicación se encuentra representado mediante las entidades y el contexto generado para trabajar con la base de datos existente.



Entre las principales entidades se encuentran:



\* Condominio

\* UnidadHabitacional

\* Usuario

\* Rol

\* Pago

\* CuotaMantenimiento

\* ContratoAlquiler

\* AreaComun

\* ReservaAreaComun

\* IncidenciaMantenimiento

\* Documento

\* Aviso

\* SolicitudIngreso

\* TransaccionFinanciera



\## 🔐 Seguridad y configuración



Las credenciales y valores sensibles de configuración \*\*no se almacenan en el repositorio\*\*.



El proyecto utiliza \*\*.NET User Secrets\*\* para almacenar información como:



\* Cadena de conexión a SQL Server.

\* Clave secreta de Google reCAPTCHA.



El archivo `appsettings.json` incluido en el repositorio contiene únicamente valores vacíos para estas configuraciones sensibles.



\## 🚀 Instalación



\### Requisitos



Antes de ejecutar el proyecto necesitas:



\* .NET SDK 10

\* SQL Server

\* Visual Studio 2026 o un entorno compatible con .NET

\* Git



\### 1. Clonar el repositorio



```bash

git clone https://github.com/wilfridoantoniorojasmota-blip/GAC-Condominium.git

cd GAC-Condominium

```



\### 2. Configurar la conexión a la base de datos



Desde la carpeta `PROTOTIPO`, configura el secreto correspondiente:



```bash

dotnet user-secrets set "ConnectionStrings:DefaultConnection" "TU\_CADENA\_DE\_CONEXION"

```



\### 3. Configurar reCAPTCHA



Si deseas utilizar la funcionalidad correspondiente:



```bash

dotnet user-secrets set "Recaptcha:SecretKey" "TU\_CLAVE\_SECRETA"

```



\### 4. Ejecutar el proyecto



```bash

dotnet run --project PROTOTIPO

```



También puedes abrir la solución `GAC\_C.slnx` desde Visual Studio y ejecutar el proyecto `PROTOTIPO`.



\## 📊 Estado del proyecto



\*\*Proyecto académico / portafolio — funcional\*\*



El sistema fue desarrollado como proyecto de ingeniería de software y posteriormente preparado para formar parte de un portafolio profesional.



\## 👨‍💻 Autor



\*\*Wilfrido Antonio Rojas Mota\*\*



Ingeniero de Software | Desarrollador Web Junior



\[GitHub](https://github.com/wilfridoantoniorojasmota-blip)
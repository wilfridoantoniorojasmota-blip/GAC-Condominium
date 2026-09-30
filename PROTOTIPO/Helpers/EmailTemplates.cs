namespace PROTOTIPO.Helpers
{
    public static class EmailTemplates
    {
        public static string Aprobada(string nombre)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
<meta charset='UTF-8'>
</head>

<body style='margin:0;padding:0;background:#eef2f7;font-family:Segoe UI,Arial,sans-serif;'>

<table width='100%' cellpadding='0' cellspacing='0' style='background:#eef2f7;padding:40px 0;'>

<tr>
<td align='center'>

<table width='650' cellpadding='0' cellspacing='0'
style='background:#ffffff;border-radius:18px;overflow:hidden;
box-shadow:0 8px 35px rgba(0,0,0,.08);'>

<!-- HEADER -->
<tr>
<td align='center'
style='background:linear-gradient(135deg,#0d6efd,#3a86ff);
padding:45px 30px;'>

<div style='font-size:60px;'>🎉</div>

<h1 style='margin:15px 0 5px;
color:white;
font-size:32px;
font-weight:700;'>

¡Solicitud Aprobada!

</h1>

<p style='margin:0;
color:#dce8ff;
font-size:16px;'>

Condominio Los Almendros

</p>

</td>
</tr>

<!-- BODY -->
<tr>
<td style='padding:45px;'>

<p style='font-size:17px;color:#444;line-height:30px;'>

Hola <strong>{nombre}</strong>,

</p>

<p style='font-size:16px;
color:#555;
line-height:30px;'>

Nos complace informarte que tu solicitud de ingreso al
<strong>Condominio Los Almendros</strong>
ha sido revisada por nuestro equipo administrativo y ha sido
<strong style='color:#198754;'>APROBADA</strong>.

</p>

<div style='margin:35px 0;
padding:25px;
background:#f8fff9;
border-left:6px solid #28a745;
border-radius:10px;'>

<h3 style='margin-top:0;
color:#198754;'>

✔ Estado de la solicitud

</h3>

<p style='margin-bottom:0;
color:#555;
line-height:28px;'>

Tu proceso de admisión ha finalizado satisfactoriamente.
Muy pronto un administrador se pondrá en contacto contigo para
completar los pasos finales de incorporación.

</p>

</div>

<h3 style='color:#0d6efd;'>¿Qué sucede ahora?</h3>

<ul style='color:#555;
font-size:15px;
line-height:30px;
padding-left:20px;'>

<li>Un administrador verificará la información suministrada.</li>

<li>Se coordinará contigo la entrega de documentación si fuese necesaria.</li>

<li>Recibirás las instrucciones para acceder a la plataforma del condominio.</li>

<li>Podrás comenzar a disfrutar de todos los servicios y beneficios disponibles.</li>

</ul>

<div style='text-align:center;margin:45px 0;'>

<a href='#'
style='background:#0d6efd;
color:white;
padding:16px 36px;
text-decoration:none;
font-size:16px;
border-radius:8px;
display:inline-block;
font-weight:600;'>

Ver más información

</a>

</div>

<p style='font-size:15px;
color:#555;
line-height:28px;'>

Queremos darte la bienvenida a nuestra comunidad y agradecerte la confianza depositada en nosotros.

</p>

<p style='font-size:15px;
color:#555;
line-height:28px;'>

Atentamente,<br>

<strong>Administración del Condominio Los Almendros</strong>

</p>

</td>
</tr>

<!-- FOOTER -->
<tr>
<td align='center'
style='background:#f7f9fc;
padding:30px;
border-top:1px solid #ececec;'>

<p style='margin:0;
font-size:13px;
color:#888;'>

Este es un correo generado automáticamente.<br>

Por favor, no respondas a este mensaje.

</p>

<p style='margin-top:18px;
font-size:12px;
color:#b3b3b3;'>

© " + DateTime.Now.Year + @" Condominio Los Almendros.<br>
Todos los derechos reservados.

</p>

</td>
</tr>

</table>

</td>
</tr>

</table>

</body>
</html>";
        }

        public static string Rechazada(string nombre)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
<meta charset='UTF-8'>
</head>

<body style='margin:0;padding:0;background:#eef2f7;font-family:Segoe UI,Arial,sans-serif;'>

<table width='100%' cellpadding='0' cellspacing='0' style='background:#eef2f7;padding:40px 0;'>

<tr>
<td align='center'>

<table width='650' cellpadding='0' cellspacing='0'
style='background:#ffffff;border-radius:18px;overflow:hidden;
box-shadow:0 8px 35px rgba(0,0,0,.08);'>

<!-- HEADER -->
<tr>
<td align='center'
style='background:linear-gradient(135deg,#dc3545,#b02a37);
padding:45px 30px;'>

<div style='font-size:60px;'>❌</div>

<h1 style='margin:15px 0 5px;
color:white;
font-size:32px;
font-weight:700;'>

Solicitud no aprobada

</h1>

<p style='margin:0;
color:#ffd9dc;
font-size:16px;'>

Condominio Los Almendros

</p>

</td>
</tr>

<!-- BODY -->
<tr>
<td style='padding:45px;'>

<p style='font-size:17px;color:#444;line-height:30px;'>

Hola <strong>{nombre}</strong>,

</p>

<p style='font-size:16px;
color:#555;
line-height:30px;'>

Te informamos que, luego de revisar tu solicitud de ingreso al
<strong>Condominio Los Almendros</strong>,
la administración ha decidido
<strong style='color:#dc3545;'>no aprobarla en esta ocasión.</strong>

</p>

<div style='margin:35px 0;
padding:25px;
background:#fff6f6;
border-left:6px solid #dc3545;
border-radius:10px;'>

<h3 style='margin-top:0;
color:#dc3545;'>

Información importante

</h3>

<p style='margin-bottom:0;
color:#555;
line-height:28px;'>

Esta decisión responde a los criterios internos establecidos por la administración del condominio y no necesariamente constituye una decisión definitiva para futuras solicitudes.

</p>

</div>

<h3 style='color:#dc3545;'>¿Qué puedes hacer?</h3>

<ul style='color:#555;
font-size:15px;
line-height:30px;
padding-left:20px;'>

<li>Verificar que toda la información suministrada sea correcta.</li>

<li>Esperar una nueva convocatoria o período de admisión, si aplica.</li>

<li>Comunicarte con la administración si necesitas aclarar alguna duda.</li>

</ul>

<div style='text-align:center;margin:45px 0;'>

<span
style='background:#f8f9fa;
color:#666;
padding:16px 36px;
border-radius:8px;
display:inline-block;
font-size:15px;
font-weight:600;
border:1px solid #dee2e6;'>

Gracias por tu interés en nuestra comunidad.

</span>

</div>

<p style='font-size:15px;
color:#555;
line-height:28px;'>

Agradecemos el tiempo dedicado a completar el proceso de solicitud y esperamos poder contar contigo en futuras oportunidades.

</p>

<p style='font-size:15px;
color:#555;
line-height:28px;'>

Atentamente,<br>

<strong>Administración del Condominio Los Almendros</strong>

</p>

</td>
</tr>

<!-- FOOTER -->
<tr>
<td align='center'
style='background:#f7f9fc;
padding:30px;
border-top:1px solid #ececec;'>

<p style='margin:0;
font-size:13px;
color:#888;'>

Este es un correo generado automáticamente.<br>

Por favor, no respondas a este mensaje.

</p>

<p style='margin-top:18px;
font-size:12px;
color:#b3b3b3;'>

© " + DateTime.Now.Year + @" Condominio Los Almendros.<br>
Todos los derechos reservados.

</p>

</td>
</tr>

</table>

</td>
</tr>

</table>

</body>
</html>";
        }

        public static string SolicitudRecibida(string nombre)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
<meta charset='UTF-8'>
</head>

<body style='margin:0;padding:0;background:#eef2f7;font-family:Segoe UI,Arial,sans-serif;'>

<table width='100%' cellpadding='0' cellspacing='0'
       style='background:#eef2f7;padding:40px 0;'>

<tr>
<td align='center'>

<table width='650' cellpadding='0' cellspacing='0'
       style='background:#ffffff;border-radius:18px;overflow:hidden;
       box-shadow:0 8px 35px rgba(0,0,0,.08);'>

<!-- HEADER -->
<tr>
<td align='center'
    style='background:linear-gradient(135deg,#0d6efd,#3a86ff);
    padding:45px 30px;'>

<div style='font-size:60px;'>📩</div>

<h1 style='margin:15px 0 5px;
color:white;
font-size:32px;
font-weight:700;'>

¡Solicitud recibida!

</h1>

<p style='margin:0;
color:#dce8ff;
font-size:16px;'>

Condominio Los Almendros

</p>

</td>
</tr>

<!-- BODY -->
<tr>
<td style='padding:45px;'>

<p style='font-size:17px;color:#444;line-height:30px;'>

Hola <strong>{nombre}</strong>,

</p>

<p style='font-size:16px;
color:#555;
line-height:30px;'>

Hemos recibido correctamente tu solicitud de ingreso al
<strong>Condominio Los Almendros</strong>.

</p>

<div style='margin:35px 0;
padding:25px;
background:#f5f9ff;
border-left:6px solid #0d6efd;
border-radius:10px;'>

<h3 style='margin-top:0;
color:#0d6efd;'>

✔ Solicitud registrada

</h3>

<p style='margin-bottom:0;
color:#555;
line-height:28px;'>

Tu solicitud ha sido registrada satisfactoriamente y actualmente
se encuentra <strong>en proceso de revisión</strong> por parte
de la administración.

</p>

</div>

<h3 style='color:#0d6efd;'>¿Qué sucede ahora?</h3>

<ul style='color:#555;
font-size:15px;
line-height:30px;
padding-left:20px;'>

<li>La administración revisará la información suministrada.</li>

<li>Se evaluará tu solicitud de acuerdo con los criterios establecidos.</li>

<li>Una vez finalizado el proceso recibirás una notificación por correo.</li>

<li>Se te informará si la solicitud fue aprobada o no aprobada.</li>

</ul>

<div style='text-align:center;margin:45px 0;'>

<span
style='background:#eaf2ff;
color:#0d6efd;
padding:16px 36px;
border-radius:8px;
display:inline-block;
font-size:15px;
font-weight:600;
border:1px solid #cfe0ff;'>

Estado actual: En revisión

</span>

</div>

<p style='font-size:15px;
color:#555;
line-height:28px;'>

No necesitas realizar ninguna acción adicional por el momento.
Te recomendamos estar atento/a a tu correo electrónico para
recibir la respuesta de la administración.

</p>

<p style='font-size:15px;
color:#555;
line-height:28px;'>

Agradecemos tu interés en formar parte de nuestra comunidad.

</p>

<p style='font-size:15px;
color:#555;
line-height:28px;'>

Atentamente,<br>

<strong>Administración del Condominio Los Almendros</strong>

</p>

</td>
</tr>

<!-- FOOTER -->
<tr>
<td align='center'
    style='background:#f7f9fc;
    padding:30px;
    border-top:1px solid #ececec;'>

<p style='margin:0;
font-size:13px;
color:#888;'>

Este es un correo generado automáticamente.<br>

Por favor, no respondas a este mensaje.

</p>

<p style='margin-top:18px;
font-size:12px;
color:#b3b3b3;'>

© " + DateTime.Now.Year + @" Condominio Los Almendros.<br>
Todos los derechos reservados.

</p>

</td>
</tr>

</table>

</td>
</tr>

</table>

</body>
</html>";
        }
    }
}

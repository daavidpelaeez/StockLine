<img width="500" height="500" alt="stockline " src="https://github.com/user-attachments/assets/d5e1e441-41fc-4093-bb31-4fe3c914e7f3" />
---


## 👤 Autor
**David Pelaez**  
Proyecto de 2º DAM
![GitHub repo size](https://img.shields.io/github/repo-size/daavidpelaeez/StockLine?style=flat-square) 
---

## 📖 Descripción

**Stock Line** es un sistema completo para la gestión de inventario, reparaciones y envíos de materiales tecnológicos en la empresa **Vialine**, proveedora de suministros y dispositivos a cuerpos de seguridad municipales en España.  

El proyecto surge como respuesta a los problemas del sistema anterior basado en hojas de cálculo, que provocaba errores frecuentes, falta de actualización en tiempo real y pérdida de información.

Stock Line centraliza todos los procesos en una **plataforma moderna, confiable y accesible desde cualquier dispositivo**.

---

## 💡 Origen de la Idea

La idea nació de la experiencia directa en Vialine, donde se detectaban dificultades para mantener actualizada la información de equipos como PDAs, impresoras portátiles, rollos de papel, pruebas de drogas y otros dispositivos utilizados por la policía local.

El proyecto busca **eliminar duplicaciones, pérdida de datos y falta de trazabilidad**, integrando todos los procesos en un solo entorno digital.

---

## 🏗 Arquitectura y Tecnología

| Componente         | Tecnología / Frameworks                             | Función Principal                                        |
|------------------|-----------------------------------------------------|---------------------------------------------------------|
| Escritorio        | C# + WPF (.NET Framework 4.8)                     | Gestión de inventario, reparaciones y envíos           |
| API REST          | Java + Spring Boot                                 | Comunicación entre base de datos y apps                |
| Aplicación móvil  | Android Studio + Jetpack Compose                   | Registro de solicitudes de material por comerciales   |
| Base de datos     | MySQL                                              | Almacenamiento de inventario, reparaciones, envíos    |

---

## ⚙ Funcionalidades Principales

- **Gestión centralizada del inventario**: actualización en tiempo real de todos los dispositivos y materiales.  
- **Control de reparaciones**: registro de incidencias, tiempos de resolución y estado actual de cada equipo.  
- **Seguimiento de envíos**: trazabilidad completa y reportes automáticos hacia los distintos ayuntamientos.  
- **Aplicación móvil para comerciales**: registro de solicitudes desde cualquier lugar.  
- **Interfaz intuitiva**: diseñada para minimizar la curva de aprendizaje.  
- **Escalabilidad**: adaptación a futuras necesidades de la empresa.

---

## 🛠 Instalación y Uso

### Escritorio
```bash
git clone https://github.com/daavidpelaeez/stock-line.git

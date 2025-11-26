import subprocess
import time
import sys

# Esperar a que SQL Server esté listo
print("Esperando a que SQL Server esté listo...")
for i in range(30):
    try:
        result = subprocess.run(
            ['docker', 'exec', 'mssql-server', '/opt/mssql-tools18/bin/sqlcmd', 
             '-S', 'localhost', '-U', 'sa', '-P', 'StrongPass123!', 
             '-Q', 'SELECT 1', '-No', '-C'],
            capture_output=True,
            timeout=10
        )
        if result.returncode == 0:
            print("SQL Server está listo!")
            break
    except:
        pass
    if i < 29:
        time.sleep(2)
        print(f"Intento {i+1}/30...")
else:
    print("Timeout esperando SQL Server")
    sys.exit(1)

# Leer el script SQL
with open('DatabaseSetup.sql', 'r') as f:
    sql_script = f.read()

# Ejecutar el script
print("Ejecutando DatabaseSetup.sql...")
result = subprocess.run(
    ['docker', 'exec', '-i', 'mssql-server', '/opt/mssql-tools18/bin/sqlcmd', 
     '-S', 'localhost', '-U', 'sa', '-P', 'StrongPass123!', '-C'],
    input=sql_script.encode(),
    capture_output=True,
    timeout=60
)

print("STDOUT:", result.stdout.decode()[:500] if result.stdout else "")
print("STDERR:", result.stderr.decode()[:500] if result.stderr else "")
print("Return code:", result.returncode)

if result.returncode == 0:
    print("Base de datos creada exitosamente!")
else:
    print("Error al ejecutar el script")
    sys.exit(1)

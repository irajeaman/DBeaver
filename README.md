Here are some notes on using **DBeaver** and **RdpFileCreator** for connecting to a database via **RDP and locally**:

---

## **DBeaver Usage Notes**
DBeaver is a database management tool that supports various databases, including Oracle, MySQL, and PostgreSQL.

### **Connecting to a Database in DBeaver**
1. **Install DBeaver**  
   - Download from [DBeaver Official Site](https://dbeaver.io/)
   - Install and launch it.

2. **Create a New Database Connection**  
   - Click **Database** → **New Connection**  
   - Choose the appropriate database type (e.g., Oracle, MySQL, PostgreSQL).
   - Enter the **host, port, database name, username, and password**.
   - Click **Test Connection** to verify.

3. **Using Connection String**
   - Instead of manually entering the credentials, you can provide a connection string:
     ```
     jdbc:oracle:thin:@//172.15.16.12:1521/XEPDB1
     ```
   - Example for MySQL:
     ```
     jdbc:mysql://172.15.16.12:3306/myDatabase?user=myUser&password=myPassword
     ```

---

## **RdpFileCreator**
This tool is used to generate **RDP files** for remote database access.

### **Usage:**
```
RdpFileCreator <IP_Address> <ApplicationName> <Username> <Password> <DB_ConnectionString>
```
### **Example:**
```
RdpFileCreator 172.28.91.133 sqldeveloper myUser myPassword "driver=Oracle|host=172.15.16.12|port=1521|database=XEPDB1|user=Username|password=password"
```

### **Steps to Connect via RDP**
1. **Run RdpFileCreator** with appropriate parameters.
2. **Open the generated `.rdp` file** to establish the remote session.
3. **Log in to the remote machine**.
4. **Launch the database application (e.g., SQL Developer, DBeaver).**
5. **Use the database connection string** to connect.

---

## **Connecting via RDP vs. Locally**
| Method  | Pros | Cons |
|---------|------|------|
| **RDP** | Secure access to a remote DB | Dependent on network latency |
| **Local** | Faster performance | Requires direct access to the database |

---

### **Best Practices**
- **Use SSH tunneling** for secure remote database connections.
- **Store credentials securely** instead of hardcoding in scripts.
- **Use VPN** when accessing databases remotely.

Let me know if you need more details! 🚀

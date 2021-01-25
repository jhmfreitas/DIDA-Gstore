# DAD
Design and Implementation of Distributed Applications Project

Script notes:
1. Included scripts have comments included within the script (which start with #).

Notes for running the project:
1. Build everything as debug. The current setup assumes the client and server .exe files are within the bin/Debug subfolders of their respective folders.
2. All client scripts must be placed in the same folder as the PCS .exe file.
3. All Puppetmaster scripts must be placed in the same folder as the Puppetmaster .exe file.
4. Only .txt files can be used for scripts. (If you want to call a script file inside a script don't provide the extension type - it will be automatically appended)
5. The puppetmaster must be shut down before starting a new configuration script.

Other remarks:
1. Servers are only created after the first non configuration command. It's advised to use wait X to let the servers start.
2. Servers are only aware of crashes if they have a master - replica relationship. Which means status command might print a "wrong" notion of dead or alive.
3. ReplicationFactor command is ignored. It's assumed that partition commands always use the same value for r.

Puppetmaster script run order (restart Puppetmaster after each script):
1. pm_basic_operations.txt
2. pm_replica_discover_master_crash.txt
3. pm_client_finds_new_master_after_crash.txt
4. pm_write_on_crashed_server.txt
5. pm_replica_master_crash.txt

Please give enough time for all scripts to run completely. **Sometimes it might seem that nothing is happening due to a longer wait**.

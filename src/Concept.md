## Registering for callbacks
1. Execute a procedure
2. Add the client to the group of that procedure
3. Await procedure response
4. Send procedure response to all subscribers of that group


Match criteria: 
* Cache time: 0 or -1 means not subscribing to a group, send individually (with cache override)
* Parameters -> Inputs, check values
  * Considering procedures never change,  outputs should always be the same.
  

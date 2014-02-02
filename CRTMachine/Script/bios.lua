print("Running bios.lua");

function printt(T) 
	for k,v in pairs(T) do
		print(k, "\t", v)
	end
end



local T = os.time()
local function delay()
	while (T == os.time()) do
		system.run() -- Make sure the system responds
	end
	T = os.time()
end

Txt = "HELLO WORLD";
while (true) do
	system.print(Txt);
	delay()
	system.clear()
	delay()
end
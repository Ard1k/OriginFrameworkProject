fx_version 'bodacious'

game 'gta5'

description 'OriginFramework'

version '0.0.5'

ui_page 'index.html'

files {
	'MenuAPI.dll',
	'Newtonsoft.Json.dll',
	'OriginFrameworkData.dll',
	'index.html',
	'config.json',
	'MySqlConnector.dll',
	'System.Buffers.dll',
	'System.Memory.dll',
	'System.Runtime.CompilerServices.Unsafe.dll',
	'System.Threading.Tasks.Extensions.dll'
}

server_scripts {
	'OriginFrameworkServer.net.dll'
}

client_scripts {
	'OriginFramework.net.dll'
}

dependencies {

}
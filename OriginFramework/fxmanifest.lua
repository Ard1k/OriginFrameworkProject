fx_version 'bodacious'

game 'gta5'

description 'OriginFramework'

version '0.0.5'

ui_page 'nui/index.html'

files {
	'MenuAPI.dll',
	'Newtonsoft.Json.dll',
	'OriginFrameworkData.dll',
	'config.json',
	'MySqlConnector.dll',
	'System.Buffers.dll',
	'System.Memory.dll',
	'System.Runtime.CompilerServices.Unsafe.dll',
	'System.Threading.Tasks.Extensions.dll',

	'nui/index.html',
	'nui/style.css',
	'nui/app.js',
	'nui/fonts/*.*'
}

server_scripts {
	'OriginFrameworkServer.net.dll'
}

client_scripts {
	'OriginFramework.net.dll'
}

dependencies {

}
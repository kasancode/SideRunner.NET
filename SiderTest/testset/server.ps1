try {
	$listener = new-object Net.HttpListener
	$listener.Prefixes.Add("http://+:8080/")
	$listener.Start()
	Write-Output "Server is running at http://localhost:8080"
	
	function ContentType ($ext) {
		switch ($ext) {
			".html" { "text/html" }
			".js" { "text/javascript" }
			".css" { "text/css" }
			".json" { "application/json" }
			".xml" { "text/xml" }
			".gif" { "image/gif" }
			".ico" { "image/x-icon" }
			".jpg" { "image/jpeg" }
			".png" { "image/png" }
			".svg" { "image/svg+xml" }
			".webp" { "image/webp" }
			".zip" { "application/zip" }
			".webp" { "image/webp" }
			Default { "text/plain" }
		}
	}

	while($true){
		$context = $listener.GetContext()
		$filepath = join-path (Get-Location) $Args[0]
		$extension = [IO.Path]::GetExtension($filepath)
		$context.Response.ContentType = ContentType($extension)
		$rstream = [IO.File]::OpenRead($filepath)
		$stream = $context.Response.OutputStream
		$rstream.CopyTo($stream)
		$stream.Close()
		$rstream.Dispose()
	}
}
catch {
	Write-Error $_.Exception
}
finally {
	$listener.Dispose()
}
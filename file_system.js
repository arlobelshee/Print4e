var fs = require('fs'),
	path_lib = require('path');

// Async helpers.
var Semaphore = function (callback, name)
{
	var self = this;
	self.callback = callback;
	self.remaining = 0;
	self.name = name;
	self.increment = function ()
	{
		self.remaining = self.remaining + 1;
	};
	self.decrement = function ()
	{
		self.remaining = self.remaining - 1;
		if (self.remaining === 0)
		{
			self.callback();
		}
	};
};

var CompletionPort = function (done, name)
{
	var self = this;
	self.processCount = new Semaphore(done, name);
	self.pendingOperations = [];
	self.started = function () { self.processCount.increment(); };
	self.done = function () { self.processCount.decrement(); };
};

var FileSystem = function (done, name)
{
	var self = this;
	self.completion = new CompletionPort(done, name);
};
FileSystem.prototype.reportError = function (err)
{
	if (err)
	{
		console.info('Error: %s', err);
		throw err;
	}
};
FileSystem.prototype.exists = function (directory, callback)
{
	var self = this;
	fs.exists(directory, function (found)
	{
		callback(found);
		self.completion.done();
	});
	self.completion.started();
};
FileSystem.prototype.forEachFile = function (directory, callback, callbackForEmptyFolder)
{
	var self = this;
	fs.readdir(directory, function (err, files)
	{
		self.reportError(err);
		if (files.length > 0)
		{
			files.forEach(callback);
		} else
		{
			callbackForEmptyFolder();
		}
		self.completion.done();
	});
	self.completion.started();
};
FileSystem.prototype.rmdir = function (directory, callback)
{
	var self = this;
	fs.rmdir(directory, function (err)
	{
		self.reportError(err);
		if (callback) { callback(); }
		self.completion.done();
	});
	self.completion.started();
};
FileSystem.prototype.mkdir = function (directory, callback)
{
	var self = this;
	callback = callback || self.reportError;
	fs.mkdir(directory, function (err)
	{
		callback(err);
		self.completion.done();
	});
	self.completion.started();
};
FileSystem.prototype.unlink = function (directory, callback)
{
	var self = this;
	fs.unlink(directory, function (err)
	{
		self.reportError(err);
		if (callback) { callback(); }
		self.completion.done();
	});
	self.completion.started();
};
FileSystem.prototype.rename = function (directory, new_name, callback)
{
	var self = this;
	var cb = callback || self.reportError;
	fs.rename(directory, new_name, function (err)
	{
		cb(err);
		self.completion.done();
	});
	self.completion.started();
};
FileSystem.prototype.writeFile = function (filename, contents)
{
	var self = this;
	fs.writeFile(filename, contents, 'binary', function (err)
	{
		self.reportError(err);
		self.completion.done();
	});
	self.completion.started();
};
FileSystem.prototype.readFile = function (filename, callback)
{
	var self = this;
	fs.readFile(filename, 'binary', function (err, contents)
	{
		self.reportError(err);
		if (callback) { callback(contents); }
		self.completion.done();
	});
	self.completion.started();
};
FileSystem.prototype.stat = function (filename, callback)
{
	var self = this;
	fs.stat(filename, function (err, stats)
	{
		self.reportError(err);
		if (callback) { callback(stats); }
		self.completion.done();
	});
	self.completion.started();
};

// Basic file system operation helpers.
var deleteFolderRecursive = function (directory, fileSystem, done)
{
	fileSystem.exists(directory, function (found)
	{
		if (!found) { return; }

		var killThisDir = function () { fileSystem.rmdir(directory, done); };
		var afterAllFilesAreDeleted = new Semaphore(killThisDir, "rmdir " + directory);
		var killOneFile = function (file, index)
		{
			afterAllFilesAreDeleted.increment();
			var child = path_lib.join(directory, file);
			fileSystem.stat(child, function (stats)
			{
				if (stats.isDirectory())
				{
					deleteFolderRecursive(child, fileSystem, afterAllFilesAreDeleted.decrement);
				} else
				{
					fileSystem.unlink(child, afterAllFilesAreDeleted.decrement);
				}
			});
		};
		fileSystem.forEachFile(directory, killOneFile, killThisDir);
	});
};

var deltree = function (directory, fileSystem) {
	var temp_name = directory + ".old";
	fileSystem.exists(temp_name, function (found) {
		if (found) {
			deleteFolderRecursive(temp_name, fileSystem);
			deleteFolderRecursive(directory, fileSystem);
		} else {
			fileSystem.rename(directory, temp_name, function (err) {
				if (!err) {
					deleteFolderRecursive(temp_name, new FileSystem(function () { }, 'background delete ' + directory));
				}
				else if (err.code === "ENOENT") {
					return;
				}
				else {
					throw err;
				}
			});
		}
	});
};

var copyFile = function (source, dest, fileSystem)
{
	fileSystem.readFile(source, function (content) { fileSystem.writeFile(dest, content); });
};

var makeDirs = function (folder, fileSystem, done)
{
	folder = path_lib.normalize(folder);
	done = done || function () { };
	if (folder.lastIndexOf(path_lib.sep) === folder.length - 1)
	{
		folder = folder.substring(0, folder.length - 1);
	}

	fileSystem.mkdir(folder, function (err)
	{
		if (!err || err.code === "EEXIST")
		{
			done();
			return;
		}
		if (err.code === "ENOENT")
		{
			var parentPathEnd = folder.lastIndexOf(path_lib.sep);
			if (parentPathEnd <= 0) { throw err; }

			makeDirs(folder.substring(0, parentPathEnd), fileSystem, function ()
			{
				fileSystem.mkdir(folder, function (err)
				{
					if (err && err.code !== "EEXIST")
					{
						throw err;
					}
					done();
				});
			});
		}
		else
		{
			throw err;
		}
	});
}

var copyFolder = function (source, dest, fileSystem)
{
	var copyContents = function ()
	{
		fileSystem.forEachFile(source, function (file, index)
		{
			var child = path_lib.join(source, file);
			fileSystem.stat(child, function (stats)
			{
				if (stats.isDirectory())
				{
					copyFolder(child, path_lib.join(dest, file), fileSystem);
				}
				else
				{
					copyFile(child, path_lib.join(dest, file), fileSystem);
				}
			});
		});
	};
	fileSystem.exists(dest, function (found)
	{
		if (found)
		{
			copyContents();
		} else
		{
			makeDirs(dest, fileSystem, copyContents);
		}
	});
};

var copyRecursive = function (source, dest, fileSystem)
{
	fileSystem.exists(source, function (found)
	{
		if (!found) { return; }
		fileSystem.stat(source, function (stats)
		{
			if (stats.isDirectory())
			{
				copyFolder(source, dest, fileSystem);
			}
			else
			{
				copyFile(source, dest, fileSystem);
			}
		});
	});
};

module.exports.FileSystem = FileSystem;
module.exports.deltree = deltree;
module.exports.copyRecursive = copyRecursive;
module.exports.makeDirs = makeDirs;
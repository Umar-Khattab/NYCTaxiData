const fs = require('fs');

const patchPath = (res) => {
  if (typeof res === 'string') {
    return res.replace(/D:([\\/])programming([\\/])c#([\\/])NYCTaxiData/gi, 'T:');
  }
  return res;
};

const originalRealpathSync = fs.realpathSync;
fs.realpathSync = function(path, options) {
  try {
    return patchPath(originalRealpathSync(path, options));
  } catch (err) {
    throw err;
  }
};

const originalRealpath = fs.realpath;
fs.realpath = function(path, options, callback) {
  let cb = callback;
  let opt = options;
  if (typeof options === 'function') {
    cb = options;
    opt = undefined;
  }
  return originalRealpath(path, opt, (err, res) => {
    if (err) return cb(err);
    cb(null, patchPath(res));
  });
};

if (fs.promises && fs.promises.realpath) {
  const originalPromisesRealpath = fs.promises.realpath;
  fs.promises.realpath = async function(path, options) {
    const res = await originalPromisesRealpath(path, options);
    return patchPath(res);
  };
}

console.log('fs.realpath monkeypatch loaded. Redirecting D:\\programming\\c#\\NYCTaxiData -> T:');

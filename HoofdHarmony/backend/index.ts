import express from "express";
import path from "path";

const app = express();
const port = 3000;

// This makes the folder accessible to the browser
// It maps the physical folder "../shared-media" to the URL "/media"
app.use("/media", express.static(path.join(__dirname, "../shared-media")));

app.listen(port, () => {
  console.log(`Ghost Server: http://localhost:${port}`);
});

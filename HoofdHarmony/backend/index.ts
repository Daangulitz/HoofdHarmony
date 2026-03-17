import express from "express";
import path from "path";

const app = express();
const port = 3000;

app.use("/media", express.static(path.join(__dirname, "../shared-media")));

app.listen(port, () => {
  console.log(`Ghost Server: http://localhost:${port}`);
});

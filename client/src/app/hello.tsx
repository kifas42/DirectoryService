"use client";

import { Button } from "@/components/ui/button";
import { useState } from "react";

export default function Hello() {
  const [counter, setCounter] = useState(0);

  const handleClick = () => {
    setCounter(counter + 1);
  };

  return (
    <div className="flex flex-col gap-4">
      <CoolText num={counter} />
      <Button variant="secondary" onClick={handleClick}>
        Увеличить
      </Button>
    </div>
  );
}

function CoolText({ num }: { num: number }) {
  return <span className="">Counter: {num}</span>;
}

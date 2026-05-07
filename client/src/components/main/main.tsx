"use client";
import useCounter from "@/hooks/use-counter";
import { Button } from "../ui/button";
import Todo from "@/app/todo";

export default function MainPage() {
  const { counter, handleClick, isWin } = useCounter();

  return (
    <>
      <div>
        <h2 className="text-3xl font-bold pb-5">
          Directory Service web-client
        </h2>
        <span>Сервис управления подразделениями</span>
      </div>
      <div>
        <span>Count {counter}</span>
        <Button onClick={handleClick}>UP</Button>
        {isWin && <span>U WIN</span>}
      </div>

      <Todo />
    </>
  );
}

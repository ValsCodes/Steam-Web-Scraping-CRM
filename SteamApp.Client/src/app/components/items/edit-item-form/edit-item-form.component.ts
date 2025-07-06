import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute } from '@angular/router';

import { ItemService } from '../../../services/item/item.service';
import { UpdateItem } from '../../../models/item.model';
import { Class, classesCollection, classesMap } from '../../../models/enums/class.enum';
import { Slot, slotsCollection, slotsMap } from '../../../models/enums/slot.enum';

@Component({
  selector: 'steam-edit-item-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './edit-item-form.component.html',
  styleUrl: './edit-item-form.component.scss'
})
export class EditItemFormComponent implements OnInit {
  itemForm!: FormGroup;

  itemId!: number;

    classes = classesCollection;
    getClassLabel(id: Class) {
      console.log(classesMap[id])
      return classesMap[id];
    }
  
    slots = slotsCollection;
  
    getSlotLabel(id: Slot) {
      return slotsMap[id];
    }

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private itemService: ItemService
  ) {}

  ngOnInit(): void {
    this.itemId = Number(this.route.snapshot.paramMap.get('id'));

    this.itemForm = this.fb.group({
      name: ['', Validators.required],
        classId: [null],
        slotId: [null],
        isActive: [false],
        isWeapon: [false],
    });

    this.itemService.getItemById(this.itemId).subscribe((product) => {
      this.itemForm.patchValue({
        name: product.name,
        classId: product.classId,
        slotId: product.slotId,
        isActive: product.isActive,
        isWeapon: product.isWeapon,
      });
    });
  }



  backButtonClicked() {
    this.router.navigate(['items-catalog']);
  }

  onSubmit(): void {
    if (this.itemForm.invalid) {
      this.itemForm.markAllAsTouched();
      return;
    }
    const updated: UpdateItem = {
      id: this.itemId,
      ...this.itemForm.value,
    };

    this.itemService.updateItem(updated).subscribe({
      next: (item) => {
        console.log('Updated item', updated);
        this.router.navigate(['items-catalog']);
      },
      error: (err) => console.error('Error updating item:', err),
    });
  }
}

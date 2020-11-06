<?php


namespace App\Form;

use Symfony\Component\Form\AbstractType;
use Symfony\Component\Form\Extension\Core\Type\CheckboxType;
use Symfony\Component\Form\Extension\Core\Type\CollectionType;
use Symfony\Component\Form\Extension\Core\Type\EmailType;
use Symfony\Component\Form\Extension\Core\Type\SubmitType;
use Symfony\Component\Form\Extension\Core\Type\TextareaType;
use Symfony\Component\Form\Extension\Core\Type\TextType;
use Symfony\Component\Form\FormBuilderInterface;
use Symfony\Component\Form\Extension\Core\Type\ChoiceType;
use Symfony\Component\OptionsResolver\OptionsResolver;



class CreateCaseFormType extends AbstractType
{

    public function buildForm(FormBuilderInterface $builder, array $options)
    {

        $builder
            ->add('userid', TextType::class, [
                'attr' => [
                    'class' => "form-control",
                ],
                'label' => 'Discord UserId'
            ])
            ->add('guildid', TextType::class, [
                'attr' => [
                    'class' => "form-control",
                ],
                'label' => 'Discord GuildId',
                'disabled' => true,
            ])
            ->add('title', TextType::class, [
                'attr' => [
                    'class' => "form-control",
                ],
                'label' => 'Title',
            ])
            ->add('labels', CollectionType::class, [
                'entry_options' => [
                    'attr' => ['class' => 'label-box list-group-item'],
                    'label' => false
                ],
                'allow_add' => true,
                'allow_delete' => true,
                'label' => false,
                'by_reference' => false,
                'prototype' => true,
                'entry_type' => TextType::class,
            ])
            ->add('description', TextareaType::class, [
                'attr' => [
                    'class' => "form-control md-textarea",
                ],
                'label' => 'Description',
            ])
            ->add('severity', TextType::class, [
                'attr' => [
                    'class' => "form-control",
                ],
                'label' => 'Severity',
                'empty_data' => '0',
                'required'=> false
            ])
            ->add('occuredAt', TextType::class, [
                'attr' => [
                    'class' => "form-control",
                ],
                'label' => 'Date & time',
                'empty_data' => date("Y-m-d\\TH:i:s.u"),
                'required'=> false
            ])
            ->add('punishment', TextType::class, [
                'attr' => [
                    'class' => "form-control",
                ],
                'label' => 'Punishment',
            ])
            ->add('sendNotification', CheckboxType::class, [
                'attr' => [
                    'class' => "form-control",
                ],
                'label' => 'sendNotification',
                'data' => true,
                'required'=> false
            ])
            ->add('submit', SubmitType::class, [
                'label' => 'CREATE CASE',
                'attr' => [
                    'class' => 'btn btn-danger btn-lg btn-block'
                ]
            ])
        ;
    }

}